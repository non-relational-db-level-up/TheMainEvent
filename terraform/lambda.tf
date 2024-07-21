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