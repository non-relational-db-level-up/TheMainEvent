using Confluent.Kafka;
using MainEvent.Hubs;
using MainEvent.Models;
using Microsoft.AspNetCore.SignalR;

namespace MainEvent.helpers;

public interface IKafkaConsumerService
{
    Task StartAsync(CancellationToken cancellationToken);
}

public class KafkaConsumerService(
    IConsumer<Null, MessageData> consumer,
    IHubContext<ChatHub> hubContext)
    : IKafkaConsumerService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // logger.LogInformation("We are spinning up a new service with DI for our R-Hub.");
        consumer.Subscribe("messages");
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // logger.LogInformation("Consume.");

                var consumeResult = consumer.Consume(cancellationToken);
                var messageData = consumeResult.Message.Value;

                // Publish the message to the SignalR hub
                await hubContext.Clients.All.SendAsync("ReceiveMessage", messageData, cancellationToken);

                Console.WriteLine(
                    $"Consumed event from topic messages: key = {consumeResult.Message.Key,-10} value = {messageData}");
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}