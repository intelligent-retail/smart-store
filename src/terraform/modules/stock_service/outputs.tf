output "stock_command_api_function_host" {
  value = azurerm_function_app.stock_service["stock_service_command"].default_hostname
}
