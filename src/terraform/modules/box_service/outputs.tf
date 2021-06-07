output "box_api_function_host" {
  value = azurerm_function_app.box_service.default_hostname
}

output "box_api_function_app_host_keys" {
  value     = data.azurerm_function_app_host_keys.box_service.default_function_key
  sensitive = true
}
