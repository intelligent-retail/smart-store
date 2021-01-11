resource "azurerm_virtual_network" "shared" {
  name                = "vnet-${local.identifier_in_module}"
  location            = azurerm_resource_group.shared.location
  resource_group_name = azurerm_resource_group.shared.name
  address_space       = [var.vnet_address_space]
}

resource "azurerm_monitor_diagnostic_setting" "virtual_network_shared" {
  name                       = "diag-${azurerm_virtual_network.shared.name}"
  target_resource_id         = azurerm_virtual_network.shared.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.shared.id

  log {
    category = "VMProtectionAlerts"
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
