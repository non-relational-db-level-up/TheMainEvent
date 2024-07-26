using Confluent.Kafka;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;
using MainEvent.DTO;
using MainEvent.helpers;
using MainEvent.Helpers;
using MainEvent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MainEvent.Hubs
{
    public class ChatHub(Itopic topic) : Hub
    {
        [Authorize]
        public override async Task OnConnectedAsync()
        {
            if (!string.IsNullOrEmpty(topic.topic))
            {
                if (topic.endTime <= DateTime.Now)
                {
                    await base.OnConnectedAsync();
                    return;
                }

                var topicSendMessage = new
                {
                    topic = topic.topic,
                    endTime = Math.Floor((topic.endTime - DateTime.Now).TotalSeconds),
                };

                await Clients.Caller.SendAsync("StartMessage", topicSendMessage);


                var earliestConfig = new ConsumerConfig
                {
                    BootstrapServers = ConstStuff.BootstrapServers,
                    GroupId = Guid.NewGuid().ToString(),
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                };
                var consumer = new ConsumerBuilder<Null, MessageData>(earliestConfig)
                    .SetValueDeserializer(new JsonSerializable<MessageData>())
                    .Build();
                consumer.Subscribe(topic.topic);
                var flag = false;
                while (true)
                {
                    var result = consumer.Consume(5000);
                    if (result == null && flag) break;
                    if (result == null) continue;
                    flag = true;
                    await Clients.Caller.SendAsync("ReceiveMessage", result.Message.Value);
                }
            }

            await base.OnConnectedAsync();
        }
    }
}