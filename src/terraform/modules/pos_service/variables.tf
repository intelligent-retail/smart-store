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
