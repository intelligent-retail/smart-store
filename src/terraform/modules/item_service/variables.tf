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

variable "workspace_ip_address_permitted" {
  type    = string
  default = ""
}

variable "key_vault_id" {
  type = string
}

variable "vnet" {
  type = object({
    id            = string
    name          = string
    address_space = list(string)
    subnet        = list(object({
      address_prefix = string
      name = string
    }))
  })
}

variable "subnet_mask" {
  type = number
}

variable "log_analytics_workspace_id" {
  type = string
}

variable "subnets_permitted" {
  type = list(object({
    id = string
    name = string
  }))
}
