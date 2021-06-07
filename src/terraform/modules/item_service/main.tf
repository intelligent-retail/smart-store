data "azurerm_virtual_network" "shared" {
  name                = var.vnet_name
  resource_group_name = var.resource_group.name
}

data "azurerm_key_vault" "shared" {
  name                = var.key_vault_name
  resource_group_name = var.resource_group.name
}

locals {
  identifier_in_module                        = "${var.identifier}-${var.resource_name_identifier}"
  key_vault_secret_name_cosmos_db_conn_string = "itemServiceCosmosDbConnectionString"
}
