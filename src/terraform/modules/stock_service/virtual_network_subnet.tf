resource "azurerm_subnet" "stock_service" {
  name                 = "snet-${local.identifier_in_module}"
  resource_group_name  = var.resource_group.name
  virtual_network_name = data.azurerm_virtual_network.shared.name
  address_prefixes     = [var.snet_address_prefix]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.AzureCosmosDB", "Microsoft.Sql", "Microsoft.Web"]

  delegation {
    name = "Delegate to app plan"

    service_delegation {
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/action"
      ]
      name = "Microsoft.Web/serverFarms"
    }
  }
}
