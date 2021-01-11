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

locals {
  vnet_address_space     = "10.0.0.0/16"
  snet_mask              = 24
  snet_bitdiff           = 8
  snet_kinds             = ["bastion", "pos_service", "item_service"]
  snet_address_prefixies = { for index, kind in local.snet_kinds : kind => cidrsubnet(local.vnet_address_space, local.snet_bitdiff, index) }
}

module "shared" {
  source = "./modules/shared"

  identifier                  = var.identifier
  location                    = var.location
  vnet_address_space          = local.vnet_address_space
  bastion_snet_address_prefix = local.snet_address_prefixies["bastion"]
}

module "pos_service" {
  source = "./modules/pos_service"

  resource_group                 = module.shared.resource_group
  identifier                     = var.identifier
  vnet_name                      = module.shared.vnet_name
  snet_address_prefix            = local.snet_address_prefixies["pos_service"]
  key_vault_id                   = module.shared.key_vault_id
  log_analytics_workspace_id     = module.shared.log_analytics_workspace_id
  workspace_ip_address_permitted = var.workspace_ip_address_permitted
  app_service_plan               = var.app_service_plan

  depends_on = [
    module.shared
  ]
}

module "item_service" {
  source = "./modules/item_service"

  resource_group                 = module.shared.resource_group
  identifier                     = var.identifier
  vnet_name                      = module.shared.vnet_name
  snet_address_prefix            = local.snet_address_prefixies["item_service"]
  key_vault_id                   = module.shared.key_vault_id
  log_analytics_workspace_id     = module.shared.log_analytics_workspace_id
  workspace_ip_address_permitted = var.workspace_ip_address_permitted
  subnets_permitted = [
    module.pos_service.subnet
  ]
  app_service_plan = var.app_service_plan

  depends_on = [
    module.shared
  ]
}
