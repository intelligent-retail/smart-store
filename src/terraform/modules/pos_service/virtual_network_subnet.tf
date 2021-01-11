resource "azurerm_subnet" "pos_service" {
  name                                           = "snet-${local.identifier_in_module}"
  resource_group_name                            = var.resource_group.name
  virtual_network_name                           = data.azurerm_virtual_network.shared.name
  address_prefixes                               = [var.snet_address_prefix]
  enforce_private_link_endpoint_network_policies = true
  service_endpoints                              = ["Microsoft.Storage", "Microsoft.AzureCosmosDB", "Microsoft.Web"]

  delegation {
    name = "Delegate to app plan"

    service_delegation {
      name = "Microsoft.Web/serverFarms"
    }
  }
}
