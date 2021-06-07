data "azurerm_client_config" "current" {}

locals {
  module_name          = "shared"
  identifier_in_module = "${var.identifier}-${local.module_name}"
}
