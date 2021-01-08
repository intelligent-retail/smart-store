terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.41"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "main" {
  name     = "rg-${var.identifier}"
  location = var.location
}

module "shared" {
  source = "./modules/shared"

  resource_group = azurerm_resource_group.main
  identifier     = var.identifier
}

module "pos_service" {
  source = "./modules/pos_service"

  resource_group                = azurerm_resource_group.main
  identifier                    = var.identifier
  app_service_plan              = var.app_service_plan
  key_vault_id                  = module.shared.key_vault_id
  vnet                          = module.shared.vnet
  subnet_mask                   = module.shared.subnet_mask
  log_analytics_workspace_id    = module.shared.log_analytics_workspace_id
  workspace_ip_address_permitted = var.workspace_ip_address_permitted

  depends_on = [
    module.shared
  ]
}

module "item_service" {
  source = "./modules/item_service"

  resource_group                = azurerm_resource_group.main
  identifier                    = var.identifier
  app_service_plan              = var.app_service_plan
  key_vault_id                  = module.shared.key_vault_id
  vnet                          = module.shared.vnet
  subnet_mask                   = module.shared.subnet_mask
  log_analytics_workspace_id    = module.shared.log_analytics_workspace_id
  workspace_ip_address_permitted = var.workspace_ip_address_permitted
  subnets_permitted = [
    module.pos_service.subnet
  ]

  depends_on = [
    module.shared
  ]
}
