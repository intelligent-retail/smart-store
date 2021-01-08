locals {
  subnet_bitdiff                 = 8
  subnet_netnum                  = length(var.vnet.subnet)
  subnet_address_prefix          = cidrsubnet(var.vnet.address_space[0], local.subnet_bitdiff, local.subnet_netnum)
  subnet_address_prefix_existing = matchkeys(var.vnet.subnet[*].address_prefix, var.vnet.subnet[*].name, [local.identifier_in_module])
  subnet_address_prefix_actual   = length(local.subnet_address_prefix_existing) > 0 ? local.subnet_address_prefix_existing[0] : local.subnet_address_prefix
}

resource "azurerm_subnet" "item_service" {
  name                                           = local.identifier_in_module
  resource_group_name                            = var.resource_group.name
  virtual_network_name                           = var.vnet.name
  address_prefixes                               = [local.subnet_address_prefix_actual]
  enforce_private_link_endpoint_network_policies = true
  service_endpoints                              = ["Microsoft.Storage", "Microsoft.AzureCosmosDB", "Microsoft.Web"]

  delegation {
    name = "Delegate to app plan"

    service_delegation {
      name = "Microsoft.Web/serverFarms"
    }
  }
}
