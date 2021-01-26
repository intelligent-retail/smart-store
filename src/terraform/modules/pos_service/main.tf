data "azurerm_virtual_network" "shared" {
  name                = var.vnet_name
  resource_group_name = var.resource_group.name
}

locals {
  module_name          = "pos-service"
  identifier_in_module = "${var.identifier}-${local.module_name}"
}
