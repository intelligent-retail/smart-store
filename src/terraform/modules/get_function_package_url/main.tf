terraform {
  required_providers {
    github = {
      source  = "integrations/github"
      version = "4.3.0"
    }
  }
}

locals {
  owner      = "intelligent-retail"
  repository = "smart-store"
}

data "github_release" "latest" {
  owner       = local.owner
  repository  = local.repository
  retrieve_by = "latest"
}

data "http" "get_assets" {
  url = data.github_release.latest.asserts_url

  request_headers = {
    Accept = "application/vnd.github.v3+json"
  }
}
