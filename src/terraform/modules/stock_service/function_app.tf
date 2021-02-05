data "azurerm_subnet" "permitted" {
  for_each = { for snet in var.snets_permitted_to_access_function : snet.key => snet.name }

  name                 = each.value
  resource_group_name  = var.resource_group.name
  virtual_network_name = var.vnet_name

  depends_on = [
    azurerm_storage_account.stock_service,
    azurerm_key_vault_secret.stock_service_cosmosdb_conn_string
  ]
}

data "azurerm_storage_account" "for_fileshare" {
  name                = var.storage_account_for_fileshare_name
  resource_group_name = var.resource_group.name
}

locals {
  function_app_ip_restriction_priority_initial_value = 300
  ip_restrictions = [
    for index, snet in var.snets_permitted_to_access_function : {
      virtual_network_subnet_id = data.azurerm_subnet.permitted[snet.key].id
      name                      = data.azurerm_subnet.permitted[snet.key].name
      priority                  = local.function_app_ip_restriction_priority_initial_value + index
      action                    = "Allow"
    }
  ]

  asset_names = {
    stock_service_command   = "StockService.StockCommand.zip"
    stock_service_processor = "StockService.StockProcessor.zip"
    stock_service_query     = "StockService.StockQuery.zip"
  }

  functions = {
    stock_service_command = {
      name = "func-${local.identifier_in_module}-stock-command-api"
      app_settings = {
        CosmosDBConnection = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.shared.name};SecretName=${local.key_vault_secret_name_cosmos_db_conn_string};SecretVersion=${azurerm_key_vault_secret.stock_service_cosmosdb_conn_string.version})"
      }
    }
    stock_service_processor = {
      name = "func-${local.identifier_in_module}-stock-processor"
      app_settings = {
        CosmosDBConnection = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.shared.name};SecretName=${local.key_vault_secret_name_cosmos_db_conn_string};SecretVersion=${azurerm_key_vault_secret.stock_service_cosmosdb_conn_string.version})"
        KeyVaultEndpoint   = data.azurerm_key_vault.shared.vault_uri
      }
    }
    stock_service_query = {
      name = "func-${local.identifier_in_module}-stock-query-api"
      app_settings = {
        KeyVaultEndpoint = data.azurerm_key_vault.shared.vault_uri
      }
    }
  }
}

module "get_function_package_url" {
  for_each = local.asset_names
  source   = "../get_function_package_url"

  asset_name = each.value
}

resource "azurerm_function_app" "stock_service" {
  for_each = local.functions

  name                       = each.value.name
  location                   = var.resource_group.location
  resource_group_name        = var.resource_group.name
  app_service_plan_id        = azurerm_app_service_plan.stock_service.id
  storage_account_name       = azurerm_storage_account.stock_service.name
  storage_account_access_key = azurerm_storage_account.stock_service.primary_access_key
  version                    = "~3"
  https_only                 = true

  site_config {
    always_on     = true
    ftps_state    = "Disabled"
    http2_enabled = true

    cors {
      allowed_origins = ["*"]
    }

    dynamic "ip_restriction" {
      for_each = local.ip_restrictions
      content {
        virtual_network_subnet_id = ip_restriction.value["virtual_network_subnet_id"]
        name                      = ip_restriction.value["name"]
        priority                  = ip_restriction.value["priority"]
        action                    = ip_restriction.value["action"]
      }
    }

    dynamic "ip_restriction" {
      for_each = var.workspace_ip_address_permitted != "" ? [1] : []
      content {
        ip_address = "${var.workspace_ip_address_permitted}/32"
        name       = "workspace"
        priority   = local.function_app_ip_restriction_priority_initial_value + length(local.ip_restrictions)
        action     = "Allow"
      }
    }
  }

  app_settings = merge({
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING = data.azurerm_storage_account.for_fileshare.primary_connection_string
    WEBSITE_CONTENTSHARE                     = each.value.name
    FUNCTIONS_WORKER_RUNTIME                 = "dotnet"
    WEBSITE_VNET_ROUTE_ALL                   = 1
    WEBSITE_RUN_FROM_PACKAGE                 = module.get_function_package_url[each.key].download_url
    APPINSIGHTS_INSTRUMENTATIONKEY           = azurerm_application_insights.stock_service.instrumentation_key
  }, each.value.app_settings)

  identity {
    type = "SystemAssigned"
  }

  depends_on = [
    module.get_function_package_url
  ]
}

resource "azurerm_app_service_virtual_network_swift_connection" "function_app_stock_service" {
  for_each = local.functions

  app_service_id = azurerm_function_app.stock_service[each.key].id
  subnet_id      = azurerm_subnet.stock_service.id
}

resource "azurerm_monitor_diagnostic_setting" "function_app_stock_service" {
  for_each = local.functions

  name                       = "diag-${azurerm_function_app.stock_service[each.key].name}"
  target_resource_id         = azurerm_function_app.stock_service[each.key].id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "FunctionAppLogs"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }
}

resource "azurerm_key_vault_access_policy" "function_app_stock_service" {
  for_each = local.functions

  key_vault_id = data.azurerm_key_vault.shared.id
  tenant_id    = azurerm_function_app.stock_service[each.key].identity[0].tenant_id
  object_id    = azurerm_function_app.stock_service[each.key].identity[0].principal_id

  secret_permissions = [
    "get",
    "list"
  ]
}
