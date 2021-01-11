variable "identifier" {
  type = string
}

variable "location" {
  type = string
}

variable "vnet_address_space" {
  type = string
}

data "azurerm_client_config" "current" {}

locals {
  module_name          = "shared"
  identifier_in_module = "${var.identifier}-${local.module_name}"
}

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
