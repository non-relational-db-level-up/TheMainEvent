using Confluent.Kafka;
using MainEvent.Hubs;
using MainEvent.Models;
using Microsoft.AspNetCore.SignalR;

namespace MainEvent.helpers;

public interface IKafkaConsumerService
{
    Task StartAsync( CancellationToken cancellationToken);
}

public class KafkaConsumerService(
    IConsumer<Null, MessageData> consumer,
    IHubContext<ChatHub> hubContext)
    : IKafkaConsumerService
{
    public async Task StartAsync( CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                var consumeResult = consumer.Consume(cancellationToken);
                var messageData = consumeResult.Message.Value;



                // Publish the message to the SignalR hub
                await hubContext.Clients.All.SendAsync("ReceiveMessage", messageData, cancellationToken);
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}