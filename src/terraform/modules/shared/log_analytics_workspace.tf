variable "log_analytics_workspace" {
  type = object({
    sku               = string
    retention_in_days = number
  })

  default = {
    sku               = "PerGB2018"
    retention_in_days = 30
  }
}

resource "azurerm_log_analytics_workspace" "shared" {
  name                = "log-${local.identifier_in_module}"
  location            = azurerm_resource_group.shared.location
  resource_group_name = azurerm_resource_group.shared.name
  sku                 = var.log_analytics_workspace.sku
  retention_in_days   = var.log_analytics_workspace.retention_in_days
}
