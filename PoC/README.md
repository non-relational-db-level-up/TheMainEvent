# r-place-kafka
 
Docker setup

1. cd kafka-init
2. ``` docker compose up ```
3. ``` docker exec -it kafka /bin/bash ```
4. ``` cd opt/kafka/bin ```
5. ``` ./kafka-topics.sh --create --topic grid-updates --bootstrap-server localhost:9092
6. cd ..
7. Create python env
8. Install dependencies using requirements.txt
9. ```python main_v2.py ```
10. Seperate console
11. ``` python kafka_producer.py ```
12. Open index.html in browser
13. Type into producer console a message to change a block, in the form: {"row" : int, "col" : int, "colour" : [int, int, int]} 