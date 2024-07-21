locals {
  dist_dir = "../lambda"
  lambda_list = {
    "kafka-producer" = {
      handler = "kafka_producer.handler"
    }
  }
}

## Role
resource "aws_iam_role" "lambda_execution_role" {
  name               = "${var.naming_prefix}-lambda-role"
  assume_role_policy = data.aws_iam_policy_document.lambda.json
}

resource "aws_iam_policy" "lambda_access" {
  name        = "${var.naming_prefix}-lambda-policy"
  description = "Policy that grants full access to the necessary resources."
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "*"
        ],
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_access_attachment" {
  policy_arn = aws_iam_policy.lambda_access.arn
  role       = aws_iam_role.lambda_execution_role.name
}

## Planning on just creating the Lambda in the console with node

data "archive_file" "lambda" {
  type        = "zip"
  source_dir  = local.dist_dir
  output_path = "node_lambda_bundle.zip"
}

resource "aws_lambda_function" "lambda" {
  for_each      = local.lambda_list
  function_name = each.key
  timeout       = 60
  memory_size = 256
  filename    = "node_lambda_bundle.zip"
  handler     = "${each.value.handler}"

  role             = aws_iam_role.lambda_execution_role.arn
  source_code_hash = data.archive_file.lambda.output_base64sha256
  runtime          = "nodejs18.x"

  environment {
    variables = {
      TOPIC = "grid-updates" # TODO: Replace this with dynamic topic
      BROKER = "${aws_instance.kafka.public_ip}:9092"
    }
  }
}