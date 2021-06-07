variable "iothub" {
  type = object({
    sku_name     = string
    sku_capacity = string
  })

  default = {
    sku_name     = "S1"
    sku_capacity = "1"
  }
}

resource "azurerm_iothub" "box_service" {
  name                = "iot-${local.identifier_in_module}"
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name

  sku {
    name     = var.iothub.sku_name
    capacity = var.iothub.sku_capacity
  }

  route {
    name           = "AI"
    source         = "DeviceMessages"
    condition      = "IS_ARRAY($body.items)\r\nAND (($body.message_type = 'stock_start') OR ($body.message_type = 'stock_diff_trading') OR ($body.message_type = 'stock_end'))"
    endpoint_names = ["events"]
    enabled        = true
  }
}

resource "azurerm_monitor_diagnostic_setting" "iothub_box_service" {
  name                           = "diag-${azurerm_iothub.box_service.name}"
  target_resource_id             = azurerm_iothub.box_service.id
  log_analytics_workspace_id     = var.log_analytics_workspace_id
  log_analytics_destination_type = "AzureDiagnostics"

  log {
    category = "Connections"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "DeviceTelemetry"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "C2DCommands"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "DeviceIdentityOperations"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "FileUploadOperations"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "Routes"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "D2CTwinOperations"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "C2DTwinOperations"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "TwinQueries"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "JobsOperations"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "DirectMethods"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "DistributedTracing"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "Configurations"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }

  log {
    category = "DeviceStreams"
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

resource "azurerm_key_vault_secret" "box_service_iothub_conn_string" {
  name         = local.key_vault_secret_name_iothub_conn_string
  value        = "HostName=${azurerm_iothub.box_service.hostname};SharedAccessKeyName=${azurerm_iothub.box_service.shared_access_policy.0.key_name};SharedAccessKey=${azurerm_iothub.box_service.shared_access_policy.0.primary_key}"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "box_service_iothub_event_conn_string" {
  name         = local.key_vault_secret_name_iothub_event_conn_string
  value        = "Endpoint=${azurerm_iothub.box_service.event_hub_events_endpoint};SharedAccessKeyName=${azurerm_iothub.box_service.shared_access_policy.0.key_name};SharedAccessKey=${azurerm_iothub.box_service.shared_access_policy.0.primary_key};EntityPath=${azurerm_iothub.box_service.event_hub_events_path}"
  key_vault_id = data.azurerm_key_vault.shared.id
}
