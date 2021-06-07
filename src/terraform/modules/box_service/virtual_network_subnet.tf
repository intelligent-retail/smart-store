resource "azurerm_subnet" "box_service" {
  name                 = var.snet.name
  resource_group_name  = var.resource_group.name
  virtual_network_name = data.azurerm_virtual_network.shared.name
  address_prefixes     = [var.snet.address_prefix]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.AzureCosmosDB", "Microsoft.Web"]

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
