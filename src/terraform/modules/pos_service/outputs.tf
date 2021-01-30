output "subnet" {
  value = {
    id   = azurerm_subnet.pos_service.id
    name = azurerm_subnet.pos_service.name
  }
}

output "pos_api_function_host" {
  value = azurerm_function_app.pos_service.default_hostname
}
