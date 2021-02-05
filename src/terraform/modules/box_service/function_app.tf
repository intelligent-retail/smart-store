locals {
  function_app_ip_restriction_priority_initial_value = 300
  function_app_name                                  = "func-${local.identifier_in_module}"
  asset_names = {
    box_service = "BoxManagementService.zip"
  }
}

module "get_function_package_url" {
  for_each = local.asset_names
  source   = "../get_function_package_url"

  asset_name = each.value
}

data "azurerm_storage_account" "for_fileshare" {
  name                = var.storage_account_for_fileshare_name
  resource_group_name = var.resource_group.name
}

resource "azurerm_function_app" "box_service" {
  name                       = local.function_app_name
  location                   = var.resource_group.location
  resource_group_name        = var.resource_group.name
  app_service_plan_id        = azurerm_app_service_plan.box_service.id
  storage_account_name       = azurerm_storage_account.box_service.name
  storage_account_access_key = azurerm_storage_account.box_service.primary_access_key
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
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING = data.azurerm_storage_account.for_fileshare.primary_connection_string
    WEBSITE_CONTENTSHARE                     = local.function_app_name
    FUNCTIONS_WORKER_RUNTIME                 = "dotnet"
    WEBSITE_VNET_ROUTE_ALL                   = 1
    WEBSITE_RUN_FROM_PACKAGE                 = module.get_function_package_url.box_service.download_url
    APPINSIGHTS_INSTRUMENTATIONKEY           = azurerm_application_insights.box_service.instrumentation_key
    CosmosDBConnection                       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.shared.name};SecretName=${local.key_vault_secret_name_cosmos_db_conn_string};SecretVersion=${azurerm_key_vault_secret.box_service_cosmosdb_conn_string.version})"
    PosCartsUri                              = "https://${var.pos_api_function_host}/api/v1/carts/"
    PosApiKeyHeader                          = "x-functions-key"
    PosCartItemsPath                         = "items/"
    PosCartSubtotalPath                      = "subtotal/"
    PosCartPaymentsPath                      = "payments/"
    PosCartBillPath                          = "bill/"
    IotHubConnectionString                   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.shared.name};SecretName=${local.key_vault_secret_name_iothub_conn_string};SecretVersion=${azurerm_key_vault_secret.box_service_iothub_conn_string.version})"
    IotHubEventConnectionString              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.shared.name};SecretName=${local.key_vault_secret_name_iothub_event_conn_string};SecretVersion=${azurerm_key_vault_secret.box_service_iothub_event_conn_string.version})"
    NotificaitonHubConnectionStrings         = "Endpoint=sb://${azurerm_notification_hub_namespace.box_service.name}.servicebus.windows.net/;SharedAccessKeyName=${azurerm_notification_hub_authorization_rule.box_service_full_access.name};SharedAccessKey=${azurerm_notification_hub_authorization_rule.box_service_full_access.primary_access_key}"
    NotificationHubName                      = azurerm_notification_hub.box_service.name
  }

  identity {
    type = "SystemAssigned"
  }

  depends_on = [
    module.get_function_package_url
  ]
}

resource "azurerm_app_service_virtual_network_swift_connection" "function_app_box_service" {
  app_service_id = azurerm_function_app.box_service.id
  subnet_id      = azurerm_subnet.box_service.id
}

resource "azurerm_monitor_diagnostic_setting" "function_app_box_service" {
  name                       = "diag-${azurerm_function_app.box_service.name}"
  target_resource_id         = azurerm_function_app.box_service.id
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

resource "azurerm_key_vault_access_policy" "function_app_box_service" {
  key_vault_id = data.azurerm_key_vault.shared.id
  tenant_id    = azurerm_function_app.box_service.identity[0].tenant_id
  object_id    = azurerm_function_app.box_service.identity[0].principal_id

  secret_permissions = [
    "get",
    "list"
  ]
}
