output "subnet" {
  value = {
    id   = azurerm_subnet.pos_service.id
    name = azurerm_subnet.pos_service.name
  }
}
