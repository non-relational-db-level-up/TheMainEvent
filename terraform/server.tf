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

resource "aws_iam_role_policy_attachment" "server_ssm" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
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

## Key
resource "aws_key_pair" "server" {
  key_name   = "${var.naming_prefix}-key-pair"
  public_key = var.ec2_public_key
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

resource "aws_security_group" "elb_sg" {
  name        = "${var.naming_prefix}-elb-sg"
  description = "Security group for the elastic load balancer"
  vpc_id      = aws_vpc.vpc.id
}

resource "aws_vpc_security_group_ingress_rule" "elb_sg_http" {
  security_group_id = aws_security_group.elb_sg.id
  from_port         = 80
  to_port           = 80
  ip_protocol       = "tcp"
  cidr_ipv4         = "0.0.0.0/0"
}

resource "aws_vpc_security_group_ingress_rule" "elb_sg_https" {
  security_group_id = aws_security_group.elb_sg.id
  from_port         = 443
  to_port           = 443
  ip_protocol       = "tcp"
  cidr_ipv4         = "0.0.0.0/0"
}

resource "aws_vpc_security_group_egress_rule" "elb_sg_egress_http" {
  security_group_id            = aws_security_group.elb_sg.id
  from_port                    = 80
  to_port                      = 80
  ip_protocol                  = "tcp"
  referenced_security_group_id = aws_security_group.server.id
}

resource "aws_vpc_security_group_egress_rule" "elb_sg_egress_https" {
  security_group_id            = aws_security_group.elb_sg.id
  from_port                    = 443
  to_port                      = 443
  ip_protocol                  = "tcp"
  referenced_security_group_id = aws_security_group.server.id
}

## Elastic Beanstalk
resource "aws_elastic_beanstalk_application" "app" {
  name        = "${var.naming_prefix}-app"
  description = "Beanstalk application"
}

resource "aws_elastic_beanstalk_environment" "env" {
  name                = "${var.naming_prefix}-env"
  application         = aws_elastic_beanstalk_application.app.name
  solution_stack_name = "64bit Amazon Linux 2023 v4.3.4 running Docker"
  cname_prefix        = var.naming_prefix

  setting {
    namespace = "aws:ec2:vpc"
    name      = "VPCId"
    value     = aws_vpc.vpc.id
  }
  setting {
    namespace = "aws:ec2:vpc"
    name      = "ELBSubnets"
    value     = join(",", aws_subnet.public_subnets[*].id)
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
    name      = "EC2KeyName"
    value     = aws_key_pair.server.key_name
  }
  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "SecurityGroups"
    value     = aws_security_group.server.id
  }
  setting {
    namespace = "aws:autoscaling:asg"
    name      = "MaxSize"
    value     = "1"
  }
  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "LoadBalancerType"
    value     = "application"
  }
  setting {
    namespace = "aws:elbv2:loadbalancer"
    name      = "SecurityGroups"
    value     = aws_security_group.elb_sg.id
  }
  setting {
    namespace = "aws:elbv2:loadbalancer"
    name      = "IdleTimeout"
    value     = "60"
  }
  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "Protocol"
    value     = "HTTPS"
  }
  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "ListenerEnabled"
    value     = "true"
  }
  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "SSLCertificateArns"
    value     = "arn:aws:acm:eu-west-1:229582503298:certificate/8d3b343a-e59a-45a0-802a-f8c84c36346c"
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
  setting {
    namespace = "aws:elasticbeanstalk:application"
    name      = "Application Healthcheck URL"
    value     = "/"
  }
  setting {
    namespace = "aws:elasticbeanstalk:command"
    name      = "IgnoreHealthCheck"
    value     = "true"
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