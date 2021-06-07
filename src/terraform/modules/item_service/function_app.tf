data "azurerm_subnet" "permitted" {
  for_each = { for snet in var.snets_permitted_to_access_function : snet.key => snet.name }

  name                 = each.value
  resource_group_name  = var.resource_group.name
  virtual_network_name = var.vnet_name

  depends_on = [
    azurerm_storage_account.item_service,
    azurerm_key_vault_secret.item_service_cosmosdb_conn_string
  ]
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
  function_app_name = "func-${local.identifier_in_module}"
  asset_names = {
    item_service = "ItemService.ItemMaster.zip"
  }
}

module "get_function_package_url" {
  for_each = local.asset_names
  source   = "../get_function_package_url"

  asset_name = each.value
}

resource "azurerm_function_app" "item_service" {
  name                       = local.function_app_name
  location                   = var.resource_group.location
  resource_group_name        = var.resource_group.name
  app_service_plan_id        = azurerm_app_service_plan.item_service.id
  storage_account_name       = azurerm_storage_account.item_service.name
  storage_account_access_key = azurerm_storage_account.item_service.primary_access_key
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

  app_settings = {
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING = azurerm_storage_account.item_service.primary_connection_string
    WEBSITE_CONTENTSHARE                     = local.function_app_name
    FUNCTIONS_WORKER_RUNTIME                 = "dotnet"
    WEBSITE_CONTENTOVERVNET                  = 1
    WEBSITE_VNET_ROUTE_ALL                   = 1
    WEBSITE_RUN_FROM_PACKAGE                 = module.get_function_package_url.item_service.download_url
    APPINSIGHTS_INSTRUMENTATIONKEY           = azurerm_application_insights.item_service.instrumentation_key
    CosmosDBConnection                       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.shared.name};SecretName=${local.key_vault_secret_name_cosmos_db_conn_string};SecretVersion=${azurerm_key_vault_secret.item_service_cosmosdb_conn_string.version})"
  }

  identity {
    type = "SystemAssigned"
  }

  depends_on = [
    module.get_function_package_url,
    azurerm_storage_share.item_service
  ]
}

resource "azurerm_storage_share" "item_service" {
  name                 = local.function_app_name
  storage_account_name = azurerm_storage_account.item_service.name
}

resource "azurerm_app_service_virtual_network_swift_connection" "function_app_item_service" {
  app_service_id = azurerm_function_app.item_service.id
  subnet_id      = azurerm_subnet.item_service.id
}

resource "azurerm_monitor_diagnostic_setting" "function_app_item_service" {
  name                       = "diag-${azurerm_function_app.item_service.name}"
  target_resource_id         = azurerm_function_app.item_service.id
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

resource "azurerm_key_vault_access_policy" "function_app_item_service" {
  key_vault_id = data.azurerm_key_vault.shared.id
  tenant_id    = azurerm_function_app.item_service.identity[0].tenant_id
  object_id    = azurerm_function_app.item_service.identity[0].principal_id

  secret_permissions = [
    "get",
    "list"
  ]
}

data "azurerm_function_app_host_keys" "item_service" {
  name                = azurerm_function_app.item_service.name
  resource_group_name = var.resource_group.name

  depends_on = [
    azurerm_function_app.item_service
  ]
}
