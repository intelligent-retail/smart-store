variable "app_service_plan" {
  type = object({
    sku = object({
      tier = string
      size = string
    })
  })

  default = {
    sku = {
      tier = "PremiumV3"
      size = "P1V3"
    }
  }
}

resource "azurerm_app_service_plan" "pos_service" {
  name                = "plan-${local.identifier_in_module}"
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name

  sku {
    tier = var.app_service_plan.sku.tier
    size = var.app_service_plan.sku.size
  }
}
