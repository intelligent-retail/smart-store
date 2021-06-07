output "box_api_function_host" {
  value = module.box_service.box_api_function_host
}

output "box_api_function_app_host_keys" {
  value = module.box_service.box_api_function_app_host_keys
  sensitive = true
}
