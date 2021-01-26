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
  function_name = "func-${local.identifier_in_module}"
  asset_names = {
    item_service = "ItemService.ItemMaster.zip"
  }
}

module "get_function_package_url" {
  for_each = local.asset_names
  source   = "../get_function_package_url"

  asset_name = each.value
}

data "azurerm_storage_account" "for_fileshare" {
  name                = var.storage_account_for_fileshare_name
  resource_group_name = var.resource_group.name
}

resource "azurerm_function_app" "item_service" {
  name                       = local.function_name
  location                   = var.resource_group.location
  resource_group_name        = var.resource_group.name
  app_service_plan_id        = azurerm_app_service_plan.item_service.id
  storage_account_name       = azurerm_storage_account.item_service.name
  storage_account_access_key = azurerm_storage_account.item_service.primary_access_key
  version                    = "~3"
  https_only                 = true

  site_config {
    always_on     = true
    ftps_state    = "Disabled"
    http2_enabled = true

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

  app_settings = {
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING = data.azurerm_storage_account.for_fileshare.primary_connection_string
    WEBSITE_CONTENTSHARE                     = local.function_name
    FUNCTIONS_WORKER_RUNTIME                 = "dotnet"
    WEBSITE_VNET_ROUTE_ALL                   = 1
    WEBSITE_RUN_FROM_PACKAGE                 = module.get_function_package_url.item_service.download_url
  }

  depends_on = [
    module.get_function_package_url
  ]
}

resource "azurerm_app_service_virtual_network_swift_connection" "function_app_item_service" {
  app_service_id = azurerm_function_app.item_service.id
  subnet_id      = azurerm_subnet.item_service.id
}
