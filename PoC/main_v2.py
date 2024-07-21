import asyncio
import threading
import time
import websockets
from confluent_kafka import Consumer, KafkaError, TopicPartition
import websockets.sync
import websockets.sync.server


topic_name = 'grid-updates'
consumer_config = {
        'bootstrap.servers': 'localhost:9092',  # Kafka broker address
        'group.id': 'group1',
        'auto.offset.reset': 'latest',
        'enable.auto.commit': False,            # Disable auto commit to manually control commit
        'fetch.wait.max.ms': 10,                # Reduce the maximum wait time for fetching messages
        'fetch.min.bytes': 1,                   # Fetch messages as soon as they are available
    }

connected_clients = set()
consumer = Consumer(consumer_config)
tp = TopicPartition(topic_name, 0)

def handle_websocket(websocket):
    connected_clients.add(websocket)
    print(f"New client connected: {websocket.remote_address}")
    try:
        while True:
            time.sleep(1)
            message = websocket.recv()
            # Process the WebSocket message if needed
            for client in connected_clients:
                client.send(message)
    except websockets.ConnectionClosed:
        print(f"Client disconnected: {websocket.remote_address}")
        connected_clients.remove(websocket)

def consume_kafka():
    print('Kafka thread starting')
    
    # consumer.assign([tp])  # Replace with your Kafka topic
    consumer.subscribe([topic_name]) 
    while True:
        # print('Kafka thread looping')
        msg = consumer.poll(1)
        if msg is None:
            continue
        if msg.error():
            if msg.error().code() == KafkaError._PARTITION_EOF:
                continue
            else:
                print(f"Error: {msg.error()}")
        else:
            message = msg.value().decode('utf-8')
            print(f"Consumed message: {message}")
            broadcast_message(message)

def broadcast_message(message):
    if connected_clients:
        for client in connected_clients:
            print(f'Sending broadcast to {client.remote_address}')
            client.send(message)

def ws_client_start():
    print("Starting WebSocket server...")
    with websockets.sync.server.serve(handle_websocket, 'localhost', 8000) as server:
        server.serve_forever()
    print("WebSocket server started on ws://localhost:8000")

    # kafka_consumer_task = asyncio.create_task(consume_kafka())

    # await asyncio.gather(kafka_consumer_task, websocket_server.wait_closed())




if __name__ == "__main__":
    # asyncio.run(main())
    try:
        x = threading.Thread(target=ws_client_start)
        x.start()
        y = threading.Thread(target=consume_kafka)
        y.start()

    except Exception as e:
        print('e')
    finally:
        x.join()
        y.join()
        consumer.close()
        