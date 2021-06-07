output "resource_group" {
  value = azurerm_resource_group.shared
}

output "key_vault_name" {
  value = azurerm_key_vault.shared.name
}

output "vnet_name" {
  value = azurerm_virtual_network.shared.name
}

output "log_analytics_workspace_id" {
  value = azurerm_log_analytics_workspace.shared.id
}
