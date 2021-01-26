output "resource_group" {
  value = azurerm_resource_group.shared
}

output "key_vault_id" {
  value = azurerm_key_vault.shared.id
}

output "vnet_name" {
  value = azurerm_virtual_network.shared.name
}

output "log_analytics_workspace_id" {
  value = azurerm_log_analytics_workspace.shared.id
}

output "storage_account_for_fileshare_name" {
  value = azurerm_storage_account.for_fileshare.name
}
