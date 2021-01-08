locals {
  module_name          = "pos-service"
  identifier_in_module = "${var.identifier}-${local.module_name}"
}

output subnet {
  value = {
    id = azurerm_subnet.pos_service.id
    name = azurerm_subnet.pos_service.name
  }
}