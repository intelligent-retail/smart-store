locals {
  portal_ips            = ["104.42.195.92", "40.76.54.131", "52.176.6.30", "52.169.50.45", "52.187.184.26"]
  ip_addresses_to_allow = var.workspace_ip_address_permitted != "" ? concat([var.workspace_ip_address_permitted], local.portal_ips) : local.portal_ips
  ip_range_filter       = join(",", local.ip_addresses_to_allow)
}

resource "azurerm_cosmosdb_account" "item_service" {
  name                              = "cosmos-${local.identifier_in_module}"
  location                          = var.resource_group.location
  resource_group_name               = var.resource_group.name
  offer_type                        = "Standard"
  kind                              = "GlobalDocumentDB"
  ip_range_filter                   = local.ip_range_filter
  is_virtual_network_filter_enabled = true

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = var.resource_group.location
    failover_priority = 0
  }

  virtual_network_rule {
    id = azurerm_subnet.item_service.id
  }
}

resource "azurerm_monitor_diagnostic_setting" "cosmosdb_account_item_service" {
  name                       = "diag-${azurerm_cosmosdb_account.item_service.name}"
  target_resource_id         = azurerm_cosmosdb_account.item_service.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "DataPlaneRequests"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "GremlinRequests"
    enabled  = false

    retention_policy {
      days    = 0
      enabled = false
    }
  }

  log {
    category = "MongoRequests"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "QueryRuntimeStatistics"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "PartitionKeyStatistics"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "PartitionKeyRUConsumption"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "ControlPlaneRequests"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "CassandraRequests"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  metric {
    category = "Requests"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }
}

resource "azurerm_key_vault_secret" "cosmosdb_account_item_service" {
  name         = azurerm_cosmosdb_account.item_service.name
  value        = azurerm_cosmosdb_account.item_service.connection_strings[0]
  key_vault_id = var.key_vault_id
}
