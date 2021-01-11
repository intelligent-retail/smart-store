variable "bastion_snet_address_prefix" {
  type = string
}

locals {
  bastion_name        = "bastion-${local.identifier_in_module}"
  bastion_subnet_name = "AzureBastionSubnet"
}

resource "azurerm_subnet" "bastion_shared" {
  name                 = local.bastion_subnet_name
  resource_group_name  = azurerm_resource_group.shared.name
  virtual_network_name = azurerm_virtual_network.shared.name
  address_prefixes     = [var.bastion_snet_address_prefix]
}

resource "azurerm_public_ip" "bastion_shared" {
  name                = "pip-${local.identifier_in_module}-bastion"
  location            = azurerm_resource_group.shared.location
  resource_group_name = azurerm_resource_group.shared.name
  sku                 = "Standard"
  allocation_method   = "Static"
}

resource "azurerm_bastion_host" "shared" {
  name                = local.bastion_name
  location            = azurerm_resource_group.shared.location
  resource_group_name = azurerm_resource_group.shared.name

  ip_configuration {
    name                 = "ipConfigBastion"
    subnet_id            = azurerm_subnet.bastion_shared.id
    public_ip_address_id = azurerm_public_ip.bastion_shared.id
  }
}

resource "azurerm_monitor_diagnostic_setting" "bastion_shared" {
  name                       = "diag-${azurerm_bastion_host.shared.name}"
  target_resource_id         = azurerm_bastion_host.shared.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.shared.id

  log {
    category = "BastionAuditLogs"
    enabled  = true

    retention_policy {
      days    = 30
      enabled = true
    }
  }
}
