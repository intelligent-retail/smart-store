resource "azurerm_function_app" "pos_service" {
  name                       = "func-${local.identifier_in_module}"
  location                   = var.resource_group.location
  resource_group_name        = var.resource_group.name
  app_service_plan_id        = azurerm_app_service_plan.pos_service.id
  storage_account_name       = azurerm_storage_account.pos_service.name
  storage_account_access_key = azurerm_storage_account.pos_service.primary_access_key

  site_config {
    always_on = true

    cors {
      allowed_origins = ["*"]
    }

    # ip_restriction {
    #   # TODO: Set the subnet for box service
    #   # virtual_network_subnet_id = azurerm_subnet.pos_service.id
    #   # name                      = local.identifier_in_module

    #   priority                  = 300
    #   action                    = "Allow"
    # }

    dynamic "ip_restriction" {
      for_each = var.workspace_ip_address_permitted != "" ? [1] : []
      content {
        ip_address = "${var.workspace_ip_address_permitted}/32"
        name       = "workspace"
        priority   = 301
        action     = "Allow"
      }
    }
  }
}

resource "azurerm_app_service_virtual_network_swift_connection" "function_app_pos_service" {
  app_service_id = azurerm_function_app.pos_service.id
  subnet_id      = azurerm_subnet.pos_service.id
}
