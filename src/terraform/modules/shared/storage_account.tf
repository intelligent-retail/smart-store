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

resource "random_string" "for_fileshare_name" {
  length  = 22
  upper   = false
  special = false
  keepers = {
    resource_group = azurerm_resource_group.shared.id
    module         = local.module_name
  }
}

#------------------------------------------------------------------------------
# This storage exists just for file temporarily because VNet integration does not support the drive mount feature currently.
# See https://docs.microsoft.com/en-us/azure/azure-functions/functions-networking-options#restrict-your-storage-account-to-a-virtual-network-preview
#------------------------------------------------------------------------------
resource "azurerm_storage_account" "for_fileshare" {
  name                     = "st${random_string.for_fileshare_name.result}"
  location                 = azurerm_resource_group.shared.location
  resource_group_name      = azurerm_resource_group.shared.name
  account_kind             = var.storage_account.kind
  account_tier             = var.storage_account.tier
  account_replication_type = var.storage_account.replication_type
}

resource "azurerm_monitor_diagnostic_setting" "storage_account_for_fileshare" {
  name                       = "diag-${azurerm_storage_account.for_fileshare.name}"
  target_resource_id         = azurerm_storage_account.for_fileshare.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.shared.id

  metric {
    category = "Transaction"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  metric {
    category = "Capacity"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }
}
