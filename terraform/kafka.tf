## Role
resource "aws_iam_instance_profile" "kafka" {
  name = "${var.naming_prefix}-kafka-instance-profile"
  role = aws_iam_role.kafka.name
}

resource "aws_iam_role" "kafka" {
  name               = "${var.naming_prefix}-kafka-role"
  assume_role_policy = data.aws_iam_policy_document.ec2.json
}

resource "aws_iam_role_policy_attachment" "kafka" {
  policy_arn = "arn:aws:iam::aws:policy/AdministratorAccess"
  role       = aws_iam_role.kafka.name
}

resource "aws_iam_role_policy_attachment" "kafka_ssm" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
  role       = aws_iam_role.kafka.name
}

## Security group
resource "aws_security_group" "kafka" {
  name        = "${var.naming_prefix}-kafka-sg"
  description = "Security group for kafka instance"
  vpc_id      = aws_vpc.vpc.id
}

resource "aws_vpc_security_group_egress_rule" "kafka" {
  security_group_id = aws_security_group.kafka.id
  from_port         = 443
  to_port           = 443
  ip_protocol       = "tcp"
  cidr_ipv4         = "0.0.0.0/0"
}

## Instance
resource "aws_instance" "kafka" {
  ami                    = "ami-0b995c42184e99f98" # Amazon Linux 2023 AMI
  instance_type          = "t3.micro"
  subnet_id              = aws_subnet.public_subnets[0].id
  vpc_security_group_ids = [aws_security_group.kafka.id]
  user_data              = file("${path.module}/user_data/kafka.sh")

  iam_instance_profile = aws_iam_instance_profile.kafka.name

  tags = {
    Name = "${var.naming_prefix}-kafka"
  }
}

## EBS Volume
resource "aws_ebs_volume" "kafka" {
  availability_zone = aws_instance.kafka.availability_zone
  size              = 10
  tags = {
    Name = "${var.naming_prefix}-kafka-volume"
  }
}

## Attach EBS Volume
resource "aws_volume_attachment" "kafka" {
  device_name  = "/dev/sdh" # or any other available device name
  volume_id    = aws_ebs_volume.kafka.id
  instance_id  = aws_instance.kafka.id
  force_detach = true
}