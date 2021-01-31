variable "location" {
  type = string
}

variable "identifier" {
  type        = string
  description = "String that identify your resources."
}

# variable "prefix" {
#   type        = string
#   description = "Prefix string should be within 2 characters."

#   validation {
#     condition     = length(var.prefix) <= 2
#     error_message = "Prefix string should be within 2 characters."
#   }
# }

variable "vnet_address_space" {
  type    = string
  default = "10.0.0.0/16"
}

variable "workspace_ip_address_permitted" {
  type        = string
  description = "Set the IP address if you would like to access the resources from your workspace"
  default     = ""
}

variable "storage_account" {
  type = object({
    tier             = string
    kind             = string
    replication_type = string
  })
}

variable "app_service_plan" {
  type = object({
    sku = object({
      tier = string
      size = string
    })
  })
}

variable "sql_administrator_username" {
  type      = string
  sensitive = true
}

variable "sql_administrator_password" {
  type      = string
  sensitive = true
}
