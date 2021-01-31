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
