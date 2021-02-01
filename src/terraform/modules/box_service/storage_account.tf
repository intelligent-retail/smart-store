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

resource "random_string" "box_service_name" {
  length  = 22
  upper   = false
  special = false
  keepers = {
    resource_group = var.resource_group.id
    module         = local.module_name
  }
}

resource "azurerm_storage_account" "box_service" {
  name                     = "st${random_string.box_service_name.result}"
  location                 = var.resource_group.location
  resource_group_name      = var.resource_group.name
  account_kind             = var.storage_account.kind
  account_tier             = var.storage_account.tier
  account_replication_type = var.storage_account.replication_type
}

resource "azurerm_storage_account_network_rules" "box_service" {
  resource_group_name  = var.resource_group.name
  storage_account_name = azurerm_storage_account.box_service.name

  default_action             = "Deny"
  bypass                     = ["Logging", "Metrics", "AzureServices"]
  virtual_network_subnet_ids = [azurerm_subnet.box_service.id]
}

resource "azurerm_monitor_diagnostic_setting" "storage_account_box_service" {
  name                       = "diag-${azurerm_storage_account.box_service.name}"
  target_resource_id         = azurerm_storage_account.box_service.id
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

# -----------------------------------------------------------------------------
# Storage for assets (Public)
# -----------------------------------------------------------------------------
resource "azurerm_storage_account" "box_service_assets" {
  name                     = "st${substr(random_string.box_service_name.result, 0, 16)}assets"
  location                 = var.resource_group.location
  resource_group_name      = var.resource_group.name
  account_kind             = var.storage_account.kind
  account_tier             = var.storage_account.tier
  account_replication_type = var.storage_account.replication_type
  allow_blob_public_access = true
}

resource "azurerm_storage_container" "box_service_assets" {
  name                  = "item-images"
  storage_account_name  = azurerm_storage_account.box_service_assets.name
  container_access_type = "container"
}
