resource "azurerm_resource_group" "shared" {
  name     = "rg-${var.identifier}"
  location = var.location
}
