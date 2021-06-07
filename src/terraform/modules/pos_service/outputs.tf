output "pos_api_function_host" {
  value = azurerm_function_app.pos_service.default_hostname
}

output "pos_api_function_app_host_keys" {
  value = data.azurerm_function_app_host_keys.pos_service.default_function_key
  sensitive = true
}
