data "azurerm_virtual_network" "shared" {
  name                = var.vnet_name
  resource_group_name = var.resource_group.name
}

data "azurerm_key_vault" "shared" {
  name                = var.key_vault_name
  resource_group_name = var.resource_group.name
}

locals {
  module_name                                    = "box-service"
  identifier_in_module                           = "${var.identifier}-${local.module_name}"
  key_vault_secret_name_cosmos_db_conn_string    = "boxServiceCosmosDbConnectionString"
  key_vault_secret_name_iothub_conn_string       = "boxServiceIoTHubConnectionString"
  key_vault_secret_name_iothub_event_conn_string = "boxServiceIoTHubEventConnectionString"
}
