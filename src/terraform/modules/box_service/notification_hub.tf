variable "notification_hub_namespace" {
  type = object({
    sku_name = string
  })

  default = {
    sku_name = "Free"
  }
}

resource "azurerm_notification_hub_namespace" "box_service" {
  name                = "ntfns-${local.identifier_in_module}"
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name
  namespace_type      = "NotificationHub"
  sku_name            = var.notification_hub_namespace.sku_name
}

resource "azurerm_notification_hub" "box_service" {
  name                = "ntf-${local.identifier_in_module}"
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name
  namespace_name      = azurerm_notification_hub_namespace.box_service.name
}

resource "azurerm_notification_hub_authorization_rule" "box_service_full_access" {
  name                  = "box-service-full-access"
  resource_group_name   = var.resource_group.name
  notification_hub_name = azurerm_notification_hub.box_service.name
  namespace_name        = azurerm_notification_hub_namespace.box_service.name
  manage                = true
  send                  = true
  listen                = true
}

resource "azurerm_notification_hub_authorization_rule" "box_service_listen_only" {
  name                  = "box-service-listen-only"
  resource_group_name   = var.resource_group.name
  notification_hub_name = azurerm_notification_hub.box_service.name
  namespace_name        = azurerm_notification_hub_namespace.box_service.name
  manage                = false
  send                  = false
  listen                = true
}

resource "azurerm_monitor_diagnostic_setting" "notification_hub_namespace_box_service" {
  name                       = "diag-${azurerm_notification_hub_namespace.box_service.name}"
  target_resource_id         = azurerm_notification_hub_namespace.box_service.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "OperationalLogs"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }
}
