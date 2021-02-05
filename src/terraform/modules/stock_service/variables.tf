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

variable "resource_name_identifier" {
  type = string
}

variable "vnet_name" {
  type = string
}

variable "snet" {
  type = object({
    name           = string
    address_prefix = string
  })
}

variable "key_vault_name" {
  type = string
}

variable "log_analytics_workspace_id" {
  type = string
}

variable "workspace_ip_address_permitted" {
  type    = string
  default = ""
}

variable "snets_permitted_to_access_function" {
  type = list(object({
    key  = string
    name = string
  }))
}

variable "storage_account_for_fileshare_name" {
  type = string
}

variable "sql_administrator_username" {
  type      = string
  sensitive = true
}

variable "sql_administrator_password" {
  type      = string
  sensitive = true
}
