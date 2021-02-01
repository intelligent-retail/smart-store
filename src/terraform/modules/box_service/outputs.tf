output "subnet" {
  value = {
    id   = azurerm_subnet.box_service.id
    name = azurerm_subnet.box_service.name
  }
}
