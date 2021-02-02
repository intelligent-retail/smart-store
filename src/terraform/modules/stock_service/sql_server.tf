variable "sql_database" {
  type = object({
    collation                        = string
    edition                          = string
    requested_service_objective_name = string
    max_size_giga_bytes              = number
  })

  default = {
    collation                        = "Japanese_CI_AS"
    edition                          = "Standard"
    requested_service_objective_name = "S0"
    max_size_giga_bytes              = 20
  }
}

resource "azurerm_mssql_server" "stock_service" {
  name                         = "sql-${local.identifier_in_module}"
  location                     = var.resource_group.location
  resource_group_name          = var.resource_group.name
  version                      = "12.0"
  administrator_login          = var.sql_administrator_username
  administrator_login_password = var.sql_administrator_password
  minimum_tls_version          = "1.2"
}

resource "azurerm_sql_database" "stock_service" {
  name                = "sqldb-${local.identifier_in_module}" # StockBackend
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name
  server_name         = azurerm_mssql_server.stock_service.name

  collation = var.sql_database.collation

  # TODO: Consider update the edition to vCore from DTU
  # See https://docs.microsoft.com/en-gb/azure/azure-sql/database/purchasing-models
  edition                          = var.sql_database.edition
  requested_service_objective_name = var.sql_database.requested_service_objective_name
  max_size_bytes                   = var.sql_database.max_size_giga_bytes * 1073741824

  # TODO: Import script to create tables
  # import = {
  #   storage_uri = 
  #   storage_key = 
  #   storage_key_type = "StorageAccessKey"
  #   administrator_login = var.sql_administrator_username
  #   administrator_login_password = var.sql_administrator_password
  #   authentication_type = "SQL"
  # }
}

resource "azurerm_sql_firewall_rule" "stock_service" {
  name                = "AllowAccessFromAzureServices"
  resource_group_name = var.resource_group.name
  server_name         = azurerm_mssql_server.stock_service.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}

resource "azurerm_sql_virtual_network_rule" "stock_service" {
  name                = azurerm_subnet.stock_service.name
  resource_group_name = var.resource_group.name
  server_name         = azurerm_mssql_server.stock_service.name
  subnet_id           = azurerm_subnet.stock_service.id
}

resource "azurerm_monitor_diagnostic_setting" "sql_database_stock_service" {
  name                       = "diag-${azurerm_sql_database.stock_service.name}"
  target_resource_id         = azurerm_sql_database.stock_service.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "SQLInsights"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "AutomaticTuning"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "QueryStoreRuntimeStatistics"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "QueryStoreWaitStatistics"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "Errors"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "DatabaseWaitStatistics"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "Timeouts"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "Blocks"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "Deadlocks"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "DevOpsOperationsAudit"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "SQLSecurityAuditEvents"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  metric {
    category = "Basic"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  metric {
    category = "InstanceAndAppAdvanced"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  metric {
    category = "WorkloadManagement"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }
}

resource "azurerm_key_vault_secret" "stock_service_sql_database_conn_string" {
  name         = local.key_vault_secret_name_sql_database_conn_string
  value        = "Server=tcp:${azurerm_mssql_server.stock_service.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_sql_database.stock_service.name};Persist Security Info=False;User ID=${var.sql_administrator_username};Password=${var.sql_administrator_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = data.azurerm_key_vault.shared.id
}
