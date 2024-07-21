## Roles
resource "aws_iam_instance_profile" "server" {
  name = "${var.naming_prefix}-server-instance-profile"
  role = aws_iam_role.server.name
}

resource "aws_iam_role" "server" {
  name               = "${var.naming_prefix}-server-role"
  assume_role_policy = data.aws_iam_policy_document.ec2.json
}

resource "aws_iam_role_policy_attachment" "server" {
  policy_arn = "arn:aws:iam::aws:policy/AdministratorAccess"
  role       = aws_iam_role.server.name
}

resource "aws_iam_role" "eb" {
  name               = "${var.naming_prefix}-eb-service-role"
  assume_role_policy = data.aws_iam_policy_document.beanstalk.json
}

resource "aws_iam_role_policy_attachment" "eb_health" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSElasticBeanstalkEnhancedHealth"
  role       = aws_iam_role.eb.name
}

resource "aws_iam_role_policy_attachment" "eb_managed_updates" {
  policy_arn = "arn:aws:iam::aws:policy/AWSElasticBeanstalkManagedUpdatesCustomerRolePolicy"
  role       = aws_iam_role.eb.name
}

## Security group
resource "aws_security_group" "server" {
  name        = "${var.naming_prefix}-server-sg"
  description = "Security group for server instance"
  vpc_id      = aws_vpc.vpc.id
}

resource "aws_vpc_security_group_ingress_rule" "server" {
  security_group_id = aws_security_group.server.id
  from_port         = 80
  to_port           = 80
  ip_protocol       = "tcp"
  cidr_ipv4         = "0.0.0.0/0"
}

resource "aws_vpc_security_group_egress_rule" "server" {
  security_group_id = aws_security_group.server.id
  from_port         = 443
  to_port           = 443
  ip_protocol       = "tcp"
  cidr_ipv4         = "0.0.0.0/0"
}

## Elastic Beanstalk
resource "aws_elastic_beanstalk_application" "app" {
  name        = "${var.naming_prefix}-app"
  description = "Beanstalk application"
}

resource "aws_elastic_beanstalk_environment" "env" {
  name                = "${var.naming_prefix}-env"
  application         = aws_elastic_beanstalk_application.app.name
  solution_stack_name = "64bit Amazon Linux 2023 v3.1.3 running .NET 8"
  cname_prefix        = var.naming_prefix

  setting {
    namespace = "aws:ec2:vpc"
    name      = "VPCId"
    value     = aws_vpc.vpc.id
  }
  setting {
    namespace = "aws:ec2:vpc"
    name      = "Subnets"
    value     = join(",", aws_subnet.public_subnets[*].id)
  }
  setting {
    namespace = "aws:ec2:instances"
    name      = "InstanceTypes"
    value     = "t3.micro"
  }
  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "IamInstanceProfile"
    value     = aws_iam_instance_profile.server.name
  }
  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "SecurityGroups"
    value     = aws_security_group.server.id
  }
  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "EnvironmentType"
    value     = "SingleInstance"
  }
  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "ServiceRole"
    value     = aws_iam_role.eb.name
  }
  setting {
    namespace = "aws:elasticbeanstalk:healthreporting:system"
    name      = "SystemType"
    value     = "basic"
  }
  dynamic "setting" {
    for_each = var.environment_variables
    content {
      namespace = "aws:elasticbeanstalk:application:environment"
      name      = setting.key
      value     = setting.value
    }
  }
}