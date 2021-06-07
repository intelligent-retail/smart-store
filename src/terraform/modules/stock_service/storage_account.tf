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

resource "random_string" "stock_service_name" {
  length  = 22
  upper   = false
  special = false
  keepers = {
    resource_group = var.resource_group.id
    module         = local.identifier_in_module
  }
}

resource "azurerm_storage_account" "stock_service" {
  name                     = "st${random_string.stock_service_name.result}"
  location                 = var.resource_group.location
  resource_group_name      = var.resource_group.name
  account_kind             = var.storage_account.kind
  account_tier             = var.storage_account.tier
  account_replication_type = var.storage_account.replication_type
}

resource "azurerm_storage_account_network_rules" "stock_service" {
  resource_group_name  = var.resource_group.name
  storage_account_name = azurerm_storage_account.stock_service.name

  default_action             = "Deny"
  bypass                     = ["Logging", "Metrics", "AzureServices"]
  virtual_network_subnet_ids = [azurerm_subnet.stock_service.id]
  ip_rules                   = [var.workspace_ip_address_permitted]
}

resource "azurerm_monitor_diagnostic_setting" "storage_account_stock_service" {
  name                       = "diag-${azurerm_storage_account.stock_service.name}"
  target_resource_id         = azurerm_storage_account.stock_service.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

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
