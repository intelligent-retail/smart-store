resource "azurerm_application_insights" "pos_service" {
  name                = "appi-${local.identifier_in_module}"
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name
  application_type    = "web"
}
