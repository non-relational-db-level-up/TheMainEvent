data "aws_iam_policy_document" "ec2" {
  statement {
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["ec2.amazonaws.com"]
    }

    actions = ["sts:AssumeRole"]
  }
}

data "aws_iam_policy_document" "beanstalk" {
  statement {
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["elasticbeanstalk.amazonaws.com"]
    }

    actions = ["sts:AssumeRole"]
  }
}

data "aws_iam_policy_document" "bucket-policy" {
  statement {
    sid    = "AllowPublicRead"
    effect = "Allow"
    resources = [
      aws_s3_bucket.app.arn,
      "arn:aws:s3:::${aws_s3_bucket.app.bucket}/*",
    ]
    actions = ["S3:GetObject"]
    principals {
      type        = "*"
      identifiers = ["*"]
    }
  }
}