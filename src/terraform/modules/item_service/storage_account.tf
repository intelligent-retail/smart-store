variable "storage_account" {
  type = object({
    tier             = string
    kind             = string
    replication_type = string
  })

  default = {
    tier             = "Standard"
    kind             = "StorageV2"
    replication_type = "LRS"
  }
}

resource "random_string" "item_service_name" {
  length  = 22
  upper   = false
  special = false
  keepers = {
    resource_group = var.resource_group.id
    module         = local.module_name
  }
}

resource "azurerm_storage_account" "item_service" {
  name                     = "st${random_string.item_service_name.result}"
  location                 = var.resource_group.location
  resource_group_name      = var.resource_group.name
  account_kind             = var.storage_account.kind
  account_tier             = var.storage_account.tier
  account_replication_type = var.storage_account.replication_type

  network_rules {
    default_action             = "Deny"
    virtual_network_subnet_ids = [azurerm_subnet.item_service.id]
  }
}
