locals {
  ip_restriction_priority_initial_value = 300
  ip_restrictions = [
    for index, subnet in var.subnets_permitted : {
      virtual_network_subnet_id = subnet["id"]
      name                      = subnet["name"]
      priority                  = local.ip_restriction_priority_initial_value + index
      action                    = "Allow"
    }
  ]
}

resource "azurerm_function_app" "item_service" {
  name                       = "func-${local.identifier_in_module}"
  location                   = var.resource_group.location
  resource_group_name        = var.resource_group.name
  app_service_plan_id        = azurerm_app_service_plan.item_service.id
  storage_account_name       = azurerm_storage_account.item_service.name
  storage_account_access_key = azurerm_storage_account.item_service.primary_access_key

  site_config {
    always_on = true

    cors {
      allowed_origins = ["*"]
    }

    dynamic "ip_restriction" {
      for_each = local.ip_restrictions
      content {
        virtual_network_subnet_id = ip_restriction.value["virtual_network_subnet_id"]
        name                      = ip_restriction.value["name"]
        priority                  = ip_restriction.value["priority"]
        action                    = ip_restriction.value["action"]
      }
    }

    dynamic "ip_restriction" {
      for_each = var.workspace_ip_address_permitted != "" ? [1] : []
      content {
        ip_address = "${var.workspace_ip_address_permitted}/32"
        name       = "workspace"
        priority   = local.ip_restriction_priority_initial_value + length(local.ip_restrictions)
        action     = "Allow"
      }
    }
  }
}

resource "azurerm_app_service_virtual_network_swift_connection" "function_app_item_service" {
  app_service_id = azurerm_function_app.item_service.id
  subnet_id      = azurerm_subnet.item_service.id
}
