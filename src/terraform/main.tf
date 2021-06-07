terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.62"
    }
  }
}

provider "azurerm" {
  features {}
}

locals {
  modules = {
    "box_service" = {
      resource_name_identifier = "box-service"
    }
    "pos_service" = {
      resource_name_identifier = "pos-service"
    }
    "item_service" = {
      resource_name_identifier = "item-service"
    }
    "stock_service" = {
      resource_name_identifier = "stock-service"
    }
  }
  vnet_address_space     = "10.0.0.0/16"
  snet_bitdiff           = 8
  snet_kinds             = concat(["bastion"], keys(local.modules))
  snet_address_prefixies = { for index, kind in local.snet_kinds : kind => cidrsubnet(local.vnet_address_space, local.snet_bitdiff, index) }
  module_snets = {
    for key, module in local.modules : key => {
      key            = key
      name           = "snet-${var.identifier}-${module.resource_name_identifier}"
      address_prefix = local.snet_address_prefixies[key]
    }
  }
}

module "shared" {
  source = "./modules/shared"

  identifier                  = var.identifier
  location                    = var.location
  vnet_address_space          = local.vnet_address_space
  bastion_snet_address_prefix = local.snet_address_prefixies["bastion"]
}

module "box_service" {
  source = "./modules/box_service"

  resource_group                 = module.shared.resource_group
  identifier                     = var.identifier
  resource_name_identifier       = local.modules["box_service"].resource_name_identifier
  vnet_name                      = module.shared.vnet_name
  snet                           = local.module_snets["box_service"]
  key_vault_name                 = module.shared.key_vault_name
  log_analytics_workspace_id     = module.shared.log_analytics_workspace_id
  workspace_ip_address_permitted = var.workspace_ip_address_permitted
  app_service_plan               = var.app_service_plan
  pos_api_function_host          = module.pos_service.pos_api_function_host
  pos_api_function_app_host_keys = module.pos_service.pos_api_function_app_host_keys

  depends_on = [
    module.shared
  ]
}

module "pos_service" {
  source = "./modules/pos_service"

  resource_group                           = module.shared.resource_group
  identifier                               = var.identifier
  resource_name_identifier                 = local.modules["pos_service"].resource_name_identifier
  vnet_name                                = module.shared.vnet_name
  snet                                     = local.module_snets["pos_service"]
  key_vault_name                           = module.shared.key_vault_name
  log_analytics_workspace_id               = module.shared.log_analytics_workspace_id
  workspace_ip_address_permitted           = var.workspace_ip_address_permitted
  snets_permitted_to_access_function       = [local.module_snets["box_service"]]
  app_service_plan                         = var.app_service_plan
  item_api_function_host                   = module.item_service.item_api_function_host
  item_api_function_app_host_keys          = module.item_service.item_api_function_app_host_keys
  stock_command_api_function_host          = module.stock_service.stock_command_api_function_host
  stock_command_api_function_app_host_keys = module.stock_service.stock_command_api_function_app_host_keys

  depends_on = [
    module.shared
  ]
}

module "item_service" {
  source = "./modules/item_service"

  resource_group                     = module.shared.resource_group
  identifier                         = var.identifier
  resource_name_identifier           = local.modules["item_service"].resource_name_identifier
  vnet_name                          = module.shared.vnet_name
  snet                               = local.module_snets["item_service"]
  key_vault_name                     = module.shared.key_vault_name
  log_analytics_workspace_id         = module.shared.log_analytics_workspace_id
  workspace_ip_address_permitted     = var.workspace_ip_address_permitted
  snets_permitted_to_access_function = [local.module_snets["pos_service"]]
  app_service_plan                   = var.app_service_plan

  depends_on = [
    module.shared
  ]
}

module "stock_service" {
  source = "./modules/stock_service"

  resource_group                     = module.shared.resource_group
  identifier                         = var.identifier
  resource_name_identifier           = local.modules["stock_service"].resource_name_identifier
  vnet_name                          = module.shared.vnet_name
  snet                               = local.module_snets["stock_service"]
  key_vault_name                     = module.shared.key_vault_name
  log_analytics_workspace_id         = module.shared.log_analytics_workspace_id
  workspace_ip_address_permitted     = var.workspace_ip_address_permitted
  snets_permitted_to_access_function = [local.module_snets["pos_service"]]
  app_service_plan                   = var.app_service_plan
  sql_administrator_username         = var.sql_administrator_username
  sql_administrator_password         = var.sql_administrator_password

  depends_on = [
    module.shared
  ]
}
