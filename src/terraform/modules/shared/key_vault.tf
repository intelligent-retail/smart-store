variable "key_vault" {
  type = object({
    sku_name = string
  })

  default = {
    sku_name = "standard"
  }
}

resource "azurerm_key_vault" "shared" {
  name                = "kv-${local.identifier_in_module}"
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name
  sku_name            = var.key_vault.sku_name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    secret_permissions = [
      "get",
      "set",
      "purge",
      "delete"
    ]
  }
}

resource "azurerm_monitor_diagnostic_setting" "key_vault_shared" {
  name                       = "diag-${azurerm_key_vault.shared.name}"
  target_resource_id         = azurerm_key_vault.shared.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.shared.id

  log {
    category = "AuditEvent"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }
}
