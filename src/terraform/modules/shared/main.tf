data "azurerm_client_config" "current" {}

locals {
  module_name          = "shared"
  identifier_in_module = "${var.identifier}-${local.module_name}"
  subnet_mask          = 24
}

output key_vault_id {
  value = azurerm_key_vault.shared.id
}

output vnet {
  value = azurerm_virtual_network.shared
}

output subnet_mask {
  value = local.subnet_mask
}

output log_analytics_workspace_id {
  value = azurerm_log_analytics_workspace.shared.id
}
