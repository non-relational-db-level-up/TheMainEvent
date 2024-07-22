# Script to pull and run the docker container for Kafka here (remember to attach volume for data persistence)
# Pull the Kafka image
docker pull apache/kafka:3.7.1

# Retrieve the public IP address of the EC2 instance
# https://stackoverflow.com/questions/38679346/get-public-ip-address-on-current-ec2-instance
PUBLIC_IP_ADDRESS=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)

# Run the Kafka container
docker run -h zookeeper --name zookeeper -p 2181:2181 -e ZOOKEEPER_CLIENT_PORT=2181 -e ZOOKEEPER_TICK_TIME=2000 confluentinc/cp-zookeeper:7.4.0
docker run -h broker --name broker -p 29092:29092 -e KAFKA_BROKER_ID=1 -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 -e KAFKA_LISTENER_SECURITY_PROTOCOL_MAP=PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://broker:9092,PLAINTEXT_HOST://$PUBLIC_IP_ADDRESS:29092 -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 -e KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS=0 -e KAFKA_TRANSACTION_STATE_LOG_MIN_ISR=1 -e KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR=1 confluentinc/cp-kafka:7.4.0
docker run -h ksqldb-server --name ksqldb-server -p 8088:8088 -e KSQL_LISTENERS=http://0.0.0.0:8088 -e KSQL_BOOTSTRAP_SERVERS=broker:9092 -e KSQL_KSQL_LOGGING_PROCESSING_STREAM_AUTO_CREATE=true -e KSQL_KSQL_LOGGING_PROCESSING_TOPIC_AUTO_CREATE=true confluentinc/ksqldb-server:0.29.0
# unsure about how to attach volume for data persistence, cannot find how to specify the kafka service to use an external volume