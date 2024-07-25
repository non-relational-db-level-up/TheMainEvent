using System.Reactive.Linq;
using System.Security.Claims;
using System.Threading;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using MainEvent.DTO;
using MainEvent.helpers;
using MainEvent.Helpers;
using MainEvent.Hubs;
using MainEvent.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MainEvent.Api;

public static class Endpoints
{
    private static readonly Dictionary<string, DateTime> PreventionMap = new();
    private static DateTime _endGameTime;
    
    
    public static void ResisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetAllBlocksCurrent);
        app.MapPost("/", AddBlock);
        app.MapPost("/admin", StartNewSession)
            .RequireAuthorization("admin_auth_policy");
        app.MapPost("/test", Test);
        app.MapGet("/topics", getTopics);
        app.MapPost("/events", getTopicEvents);
    }

    private static List<MessageData> Test(
        ILogger<Program> logger,
        IKSqlDBContext context
    )
    {
        var earliestConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:29092",
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        var consumer = new ConsumerBuilder<Null, MessageData>(earliestConfig)
            .SetValueDeserializer(new JsonSerializable<MessageData>())
            .Build();
        consumer.Subscribe("cool");

        var messages = new List<MessageData>();
        var flag = false;
        while (true)
        {
            var result = consumer.Consume(5000);
            if (result == null && flag) break;
            if (result == null) continue;
            flag = true;
            messages.Add(result.Message.Value);
        }

        return messages;
    }

    private static async Task StartNewSession(
        Itopic topic,
        ILogger<Program> logger,
        IConsumer<Null, MessageData> consumer,
        IAdminClient adminClient,
        IKSqlDBContext context,
        IHubContext<ChatHub> hubContext,
        ClaimsPrincipal claims,
        [FromBody] CreateSessionDto sessionDto
    )
    {
        topic.topic = sessionDto.TopicName;
        topic.endTime = DateTime.Now.AddMinutes(10);

        try
        {
            await adminClient.CreateTopicsAsync([
                new TopicSpecification { Name = sessionDto.TopicName, NumPartitions = 1, ReplicationFactor = 1 }
            ]);

            var a = new
            {
                topic = topic.topic,
                endTime = (topic.endTime - DateTime.Now).TotalSeconds,
            };

            await hubContext.Clients.All.SendAsync("StartMessage", a);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        consumer.Subscribe(sessionDto.TopicName);
    }

    private static IAsyncEnumerable<MessageData> GetAllBlocksCurrent(
        ILogger<Program> logger,
        IKSqlDBContext context,
        ClaimsPrincipal claims
    )
    {
        // TODO: use sql like thingy to make call to the latest board state
        return context.CreatePullQuery<MessageData>().GetManyAsync();
    }

    private static async Task<JsonHttpResult<string>> AddBlock(
        Itopic topic,
        ILogger<Program> logger,
        IProducer<Null, MessageData> producer,
        ClaimsPrincipal claims,
        MessageDataDto data
    )
    {
        var userSid = claims.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (PreventionMap.TryGetValue(userSid, out var valueDate))
        {
            if ((DateTime.Now - valueDate).TotalSeconds < 5)
                return TypedResults.Json("You can only send a message every 5 seconds",
                    statusCode: 400);

            PreventionMap[userSid] = DateTime.Now;
        }
        else
            PreventionMap.Add(userSid, DateTime.Now);


        // TODO: figure out if we need to do this as partition or on different topic. 
        await producer.ProduceAsync(topic.topic, new Message<Null, MessageData>
        {
            Value = new MessageData(data.Row, data.Column, data.HexColour, userSid)
        });
        return TypedResults.Json("success");
    }


    private static async Task<JsonHttpResult<List<string>>> getTopics(
        ILogger<Program> logger,
        IAdminClient adminClient,
        ClaimsPrincipal claims
    )
    {
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var topicNames = metadata.Topics.Select(a => a.Topic).ToList();
        return TypedResults.Json(topicNames);
        
    }

    private static async Task<JsonHttpResult<List<MessageData>>> getTopicEvents(
        ILogger<Program> logger,
        IAdminClient adminClient,
        ClaimsPrincipal claims,
        GetTopicEventsDto data
    )
    {
        
        var earliestConfig = new ConsumerConfig
        {
            BootstrapServers = "54.154.112.105:29092",
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        var consumer = new ConsumerBuilder<Null, MessageData>(earliestConfig)
            .SetValueDeserializer(new JsonSerializable<MessageData>())
            .Build();
        consumer.Subscribe(data.TopicName);
        var messages = new List<MessageData>();
        var flag = false;
        while (true)
        {
            var result = consumer.Consume(5000);
            if (result == null && flag) break;
            if (result == null) continue;
            flag = true;
            messages.Add(result.Message.Value);
        }

        return  TypedResults.Json(messages);

    }
}