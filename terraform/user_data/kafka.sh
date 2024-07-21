# Script to pull and run the docker container for Kafka here (remember to attach volume for data persistence)
# Pull the Kafka image
docker pull apache/kafka:3.7.1

# Retrieve the public IP address of the EC2 instance
# https://stackoverflow.com/questions/38679346/get-public-ip-address-on-current-ec2-instance
PUBLIC_IP_ADDRESS=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)

# Run the Kafka container
docker run -d --name kafka -p 9092:9092 --env KAFKA_ADVERTISED_HOST_NAME=$PUBLIC_IP_ADDRESS --env KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 apache/kafka:3.7.1

# unsure about how to attach volume for data persistence, cannot find how to specify the kafka service to use an external volume