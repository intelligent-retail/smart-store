locals {
  consmos_db_portal_ips            = ["104.42.195.92", "40.76.54.131", "52.176.6.30", "52.169.50.45", "52.187.184.26"]
  consmos_db_ip_addresses_to_allow = var.workspace_ip_address_permitted != "" ? concat([var.workspace_ip_address_permitted], local.consmos_db_portal_ips) : local.consmos_db_portal_ips
  cosmosdb = {
    database_name       = "smartretailpos"
    database_throughput = 500
    containers = {
      carts = {
        name               = "Carts"
        partition_key_path = "/cartId"
        default_ttl        = 604800
      }
      pos_masters = {
        name               = "PosMasters"
        partition_key_path = "/mastername"
      }
      transaction_logs = {
        name               = "TransactionLogs"
        partition_key_path = "/key"
      }
      receipts = {
        name               = "Receipts"
        partition_key_path = "/key"
      }
      containers = {
        name               = "Counters"
        partition_key_path = "/terminalKey"
      }
    }
    ip_range_filter = join(",", local.consmos_db_ip_addresses_to_allow)
  }
}

resource "azurerm_cosmosdb_account" "pos_service" {
  name                              = "cosmos-${local.identifier_in_module}"
  location                          = var.resource_group.location
  resource_group_name               = var.resource_group.name
  offer_type                        = "Standard"
  kind                              = "GlobalDocumentDB"
  ip_range_filter                   = local.cosmosdb.ip_range_filter
  is_virtual_network_filter_enabled = true

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = var.resource_group.location
    failover_priority = 0
  }

  virtual_network_rule {
    id = azurerm_subnet.pos_service.id
  }
}

resource "azurerm_cosmosdb_sql_database" "pos_service" {
  name                = local.cosmosdb.database_name
  resource_group_name = var.resource_group.name
  account_name        = azurerm_cosmosdb_account.pos_service.name
  throughput          = local.cosmosdb.database_throughput
}

resource "azurerm_cosmosdb_sql_container" "pos_service" {
  for_each = local.cosmosdb.containers

  name                = each.value.name
  resource_group_name = var.resource_group.name
  account_name        = azurerm_cosmosdb_account.pos_service.name
  database_name       = azurerm_cosmosdb_sql_database.pos_service.name
  partition_key_path  = each.value.partition_key_path
  default_ttl         = try(each.value.default_ttl, null)
}

resource "azurerm_monitor_diagnostic_setting" "cosmosdb_account_pos_service" {
  name                       = "diag-${azurerm_cosmosdb_account.pos_service.name}"
  target_resource_id         = azurerm_cosmosdb_account.pos_service.id
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

resource "azurerm_key_vault_secret" "pos_service_cosmosdb_conn_string" {
  name         = local.key_vault_secret_name_cosmos_db_conn_string
  value        = azurerm_cosmosdb_account.pos_service.connection_strings[0]
  key_vault_id = data.azurerm_key_vault.shared.id
}
