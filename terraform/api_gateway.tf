## Logging
resource "aws_api_gateway_account" "api-gateway" {
  cloudwatch_role_arn = aws_iam_role.api_gateway.arn
}

resource "aws_iam_role" "api_gateway" {
  name               = "${var.naming_prefix}-api-gateway-role"
  assume_role_policy = data.aws_iam_policy_document.gateway.json
}

resource "aws_iam_role_policy_attachment" "api_gateway" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonAPIGatewayPushToCloudWatchLogs"
  role       = aws_iam_role.api_gateway.name
}

resource "aws_cloudwatch_log_group" "log_group" {
  name              = "/aws/apigateway/${aws_apigatewayv2_api.api.name}"
  retention_in_days = 7
}

## API
resource "aws_apigatewayv2_api" "api" {
  name          = "${var.naming_prefix}-service-api"
  description   = "API Gateway for passing events into Kafka"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_domain_name" "api" {
  domain_name = "api.themainevent.projects.bbdgrad.com"

  domain_name_configuration {
    certificate_arn = "arn:aws:acm:eu-west-1:229582503298:certificate/799f4407-450d-4558-b9d9-41d655150259"
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

resource "aws_apigatewayv2_stage" "api" {
  api_id      = aws_apigatewayv2_api.api.id
  name        = "$default"
  auto_deploy = true

  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.log_group.arn
    format          = "$context.requestId $context.identity.sourceIp $context.identity.caller $context.identity.user $context.requestTime $context.httpMethod $context.resourcePath $context.status $context.protocol $context.responseLength $context.error.message"
  }
}

resource "aws_apigatewayv2_api_mapping" "api" {
  api_id      = aws_apigatewayv2_api.api.id
  domain_name = aws_apigatewayv2_domain_name.api.domain_name
  stage       = aws_apigatewayv2_stage.api.id
}

resource "aws_apigatewayv2_integration" "api" {
  for_each         = var.lambda_endpoint_config
  api_id           = aws_apigatewayv2_api.api.id
  integration_type = "AWS_PROXY"

  connection_type    = "INTERNET"
  description        = each.value.description
  integration_method = each.value.method
  integration_uri    = each.value.lambda_invoke_arn
}

resource "aws_apigatewayv2_route" "api" {
  for_each  = var.lambda_endpoint_config
  api_id    = aws_apigatewayv2_api.api.id
  route_key = each.key
  target    = "integrations/${aws_apigatewayv2_integration.api[each.key].id}"
}