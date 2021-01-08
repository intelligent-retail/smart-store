locals {
  bastion_name                           = "${local.identifier_in_module}-bastion"
  bastion_subnet_name                    = "AzureBastionSubnet"
  bastion_subnet_bitdiff                 = 8
  bastion_subnet_netnum                  = length(azurerm_virtual_network.shared.subnet)
  bastion_subnet_address_prefix          = cidrsubnet(var.vnet_address_space, local.bastion_subnet_bitdiff, local.bastion_subnet_netnum)
  bastion_subnet_address_prefix_existing = matchkeys(azurerm_virtual_network.shared.subnet[*].address_prefix, azurerm_virtual_network.shared.subnet[*].name, [local.bastion_subnet_name])
  bastion_subnet_address_prefix_actual   = length(local.bastion_subnet_address_prefix_existing) > 0 ? local.bastion_subnet_address_prefix_existing[0] : local.bastion_subnet_address_prefix
}

resource "azurerm_subnet" "bastion_shared" {
  name                 = local.bastion_subnet_name
  resource_group_name  = var.resource_group.name
  virtual_network_name = azurerm_virtual_network.shared.name
  address_prefixes     = [local.bastion_subnet_address_prefix_actual]
}

resource "azurerm_public_ip" "bastion_shared" {
  name                = "pip-${local.bastion_name}"
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name
  sku                 = "Standard"
  allocation_method   = "Static"
}

resource "azurerm_bastion_host" "shared" {
  name                = local.bastion_name
  location            = var.resource_group.location
  resource_group_name = var.resource_group.name

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
