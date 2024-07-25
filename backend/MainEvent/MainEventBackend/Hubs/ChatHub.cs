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
    public class ChatHub : Hub
    {
        Itopic _topic;
        public ChatHub(Itopic topic)
        {
            _topic = topic;
        }

        [Authorize]
        public override async Task OnConnectedAsync()
        {
            if (string.IsNullOrEmpty(_topic.topic))
            {
                if (_topic.endTime <= DateTime.Now)
                {
                    await base.OnConnectedAsync();
                    return;
                }
                var a = new
                {
                    topic = _topic.topic,
                    endTime = (_topic.endTime - DateTime.Now).TotalSeconds,
                };

                await Clients.Caller.SendAsync("StartMessage", a);


                var earliestConfig = new ConsumerConfig
                {
                    BootstrapServers = "54.154.112.105:29092",
                    GroupId = Guid.NewGuid().ToString(),
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                };
                var consumer = new ConsumerBuilder<Null, MessageData>(earliestConfig)
                    .SetValueDeserializer(new JsonSerializable<MessageData>())
                    .Build();
                consumer.Subscribe(_topic.topic);
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

        //[Authorize]
        //public async Task SendMessage(string user, string message)
        //{
        //    var data = new MessageDataDto(1, 1, "#f5aa42");

        //    string jsonString = JsonSerializer.Serialize(data);


        //    await Clients.All.SendAsync("ReceiveMessage", user, jsonString);
        //}
    }
}