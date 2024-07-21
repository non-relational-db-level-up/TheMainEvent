const Kafka = require('node-rdkafka');

const topic = process.env.TOPIC;
const broker = process.env.BROKER;

function createProducer(config, onDeliveryReport) {
  const producer = new Kafka.Producer(config);

  return new Promise((resolve, reject) => {
    producer
      .on('ready', () => resolve(producer))
      .on('delivery-report', onDeliveryReport)
      .on('event.error', (err) => {
        console.warn('event.error', err);
        reject(new Error(err));
      });
    producer.connect();
  });
}

async function produceMessage(producer, topic, message) {
  return new Promise((resolve, reject) => {
    try {
      producer.produce(
        topic,
        null,
        Buffer.from(message),
        null,
        Date.now(),
        (err, offset) => {
          if (err) {
            reject(new Error(err));
        } else {
            resolve(offset);
          }
        }
      );
    } catch (err) {
        reject(new Error(err));
    }
  });
}

const config = {
    'bootstrap.servers': broker,
    'acks': 'all',
    'dr_msg_cb': true
  };

const producer = await createProducer(config, (err, report) => {
    if (err) {
      console.warn('Delivery report error:', err);
    } else {
      console.log('Delivery report:', report);
    }
  });

exports.handler = async (event, context) => {
console.log("EVENT: \n" + JSON.stringify(event, null, 2));
  try {
    const message = JSON.stringify(event);
    const offset = await produceMessage(producer, topic, message);

    return {
      statusCode: 200,
      body: JSON.stringify({
        message: 'Message produced successfully',
        offset: offset
      })
    };
  } catch (err) {
    console.error('Error producing message:', err);
    return {
      statusCode: 500,
      body: JSON.stringify({
        message: 'Failed to produce message',
        error: err.message
      })
    };
  }
};