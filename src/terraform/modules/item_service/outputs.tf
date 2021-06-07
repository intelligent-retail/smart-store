output "item_api_function_host" {
  value = azurerm_function_app.item_service.default_hostname
}

output "item_api_function_app_host_keys" {
  value = data.azurerm_function_app_host_keys.item_service.default_function_key
  sensitive = true
}
