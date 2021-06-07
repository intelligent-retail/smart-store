output "stock_command_api_function_host" {
  value = azurerm_function_app.stock_service["stock_service_command"].default_hostname
}

output "stock_command_api_function_app_host_keys" {
  value = data.azurerm_function_app_host_keys.stock_service_command_api.default_function_key
  sensitive = true
}
