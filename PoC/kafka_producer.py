from confluent_kafka import Producer
import socket
import time
topic = "grid-updates"
conf = {'bootstrap.servers': 'localhost:9092',
        'client.id': socket.gethostname()}

producer = Producer(conf)



while(True):
    temp = input("Please type in an input to the kafka stream..")
    producer.produce(topic, key="griddy", value=f"{temp}")