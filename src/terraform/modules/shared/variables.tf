variable "resource_group" {
  type = object({
    name = string
    location = string
  })
}

variable "identifier" {
  type = string
}
