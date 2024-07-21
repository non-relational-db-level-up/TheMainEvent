## User Pool
resource "aws_cognito_user_pool" "user_pool" {
  name = "${var.naming_prefix}-user-pool"
  schema {
    name                = "email"
    attribute_data_type = "String"
    mutable             = true
    required            = true
  }
  auto_verified_attributes = ["email"]
}

## User Pool Client
resource "aws_cognito_user_pool_client" "client" {
  name                                 = "${var.naming_prefix}-user-pool-client"
  user_pool_id                         = aws_cognito_user_pool.user_pool.id
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_flows                  = ["implicit", "code"]
  allowed_oauth_scopes                 = ["email", "openid", "profile", "aws.cognito.signin.user.admin"]
  callback_urls                        = var.callback_urls
  logout_urls                          = var.logout_urls
  supported_identity_providers         = ["Google"]
  generate_secret                      = false # Public client cannot store the secret securely, so we won't use one

  depends_on = [aws_cognito_identity_provider.google]
}

## User Pool Domain
resource "aws_cognito_user_pool_domain" "user_pool_domain" {
  domain       = "the-main-event"
  user_pool_id = aws_cognito_user_pool.user_pool.id
}

## Identity Provider
resource "aws_cognito_identity_provider" "google" {
  user_pool_id  = aws_cognito_user_pool.user_pool.id
  provider_name = "Google"
  provider_type = "Google"

  provider_details = {
    client_id                     = var.client_id
    client_secret                 = var.client_secret
    authorize_scopes              = "email openid profile"
    attributes_url                = "https://people.googleapis.com/v1/people/me?personFields="
    attributes_url_add_attributes = "true"
    authorize_url                 = "https://accounts.google.com/o/oauth2/v2/auth"
    oidc_issuer                   = "https://accounts.google.com"
    token_request_method          = "POST"
    token_url                     = "https://www.googleapis.com/oauth2/v4/token"
  }

  attribute_mapping = {
    username = "sub"
    email    = "email"
  }
}