variable "resource_group" {
  type = object({
    id       = string
    name     = string
    location = string
  })
}

variable "identifier" {
  type = string
}

variable "vnet_name" {
  type = string
}

variable "snet_address_prefix" {
  type = string
}

variable "key_vault_id" {
  type = string
}

variable "log_analytics_workspace_id" {
  type = string
}

variable "workspace_ip_address_permitted" {
  type    = string
  default = ""
}

variable "subnets_permitted" {
  type = list(object({
    id   = string
    name = string
  }))
  default = []
}

data "azurerm_virtual_network" "shared" {
  name                = var.vnet_name
  resource_group_name = var.resource_group.name
}

locals {
  module_name          = "pos-service"
  identifier_in_module = "${var.identifier}-${local.module_name}"
}

output "subnet" {
  value = {
    id   = azurerm_subnet.pos_service.id
    name = azurerm_subnet.pos_service.name
  }
}
