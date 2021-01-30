variable "app_service_plan" {
  type = object({
    sku = object({
      tier = string
      size = string
    })
  })

  default = {
    sku = {
      tier = "PremiumV3"
      size = "P1V3"
    }
  }
}

resource "azurerm_app_service_plan" "item_service" {
  name                = "plan-${local.identifier_in_module}"
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name

  sku {
    tier = var.app_service_plan.sku.tier
    size = var.app_service_plan.sku.size
  }
}

resource "azurerm_monitor_diagnostic_setting" "app_service_plan_item_service" {
  name                       = "diag-${azurerm_app_service_plan.item_service.name}"
  target_resource_id         = azurerm_app_service_plan.item_service.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  metric {
    category = "AllMetrics"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }
}
