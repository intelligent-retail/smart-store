output "download_url" {
  value = [for asset in jsondecode(data.http.get_assets.body) : asset if lookup(asset, "name") == var.asset_name][0].browser_download_url
}
