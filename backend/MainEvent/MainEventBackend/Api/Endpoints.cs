using System.Reactive.Linq;
using System.Security.Claims;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using MainEvent.DTO;
using MainEvent.helpers;
using MainEvent.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MainEvent.Api;

public static class Endpoints
{
    private static string _messageGroup = "";
    private static readonly Dictionary<string, DateTime> PreventionMap = new();
    private static DateTime _endGameTime;
    
    
    public static void ResisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetAllBlocksCurrent);
        app.MapPost("/", AddBlock);
        app.MapPost("/admin", StartNewSession);
        app.MapPost("/test", Test);
    }

    private static List<MessageData> Test(
        ILogger<Program> logger,
        // IConsumer<Null, MessageData> consumer,
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
        /*consumer.Subscribe("messages");
        var partitions = consumer.Assignment.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning)).ToList();
        consumer.Assign(partitions);*/

        // consumer.Subscribe("messages");
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

        // return context.CreatePullQuery<MessageData>().GetManyAsync();

        /*await context.CreateTableStatement("MESSAGEDATA").As<MessageData>().ExecuteStatementAsync();
        var asyncEnumerable = context.CreatePullQuery<MessageData>().GetManyAsync();
        await foreach (var messageData in asyncEnumerable)
        {
            Console.WriteLine(messageData);
        }*/

        // consumer.PositionTopicPartitionOffset()

        /*
        var createStatement = context.CreateTableStatement("MESSAGEDATA")
            .As<MessageData>();
        // var createStatement = context.CreateOrReplaceTableStatement("messages").As<MessageData>();
        await createStatement.ExecuteStatementAsync();
        var asyncEnumerable = context.CreatePullQuery<MessageData>().GetManyAsync();
        await foreach (var messageData in asyncEnumerable)
        {
            Console.WriteLine(messageData);
        }
        */


        // var consumeResult = consumer.Consume();
        // consumer.

        // Console.WriteLine(consumeResult);
        // consumer.Subscribe("messages");
        // Register new topic/partition
        // Send clear state on WS 
        // Register our table on ksql
        // context.CreateOrReplaceTableStatement(sessionDto.TopicName).As<MessageData>();
        /*await adminClient.CreateTopicsAsync([
            new TopicSpecification { Name = "messages", NumPartitions = 1, ReplicationFactor = 1 }
        ]);*/

        // Start listening and publishing on new 
    }

    private static async Task StartNewSession(
        ILogger<Program> logger,
        IConsumer<Null, MessageData> consumer,
        IAdminClient adminClient,
        IKSqlDBContext context,
        ClaimsPrincipal claims,
        [FromBody] CreateSessionDto sessionDto
    )
    {
        _messageGroup = sessionDto.TopicName;
        try
        {
            await adminClient.CreateTopicsAsync([
                new TopicSpecification { Name = sessionDto.TopicName, NumPartitions = 1, ReplicationFactor = 1 }
            ]);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        consumer.Subscribe(sessionDto.TopicName);
        /*var creationMetadata = new CreationMetadata
        {
            KafkaTopic = "cool",
            ValueFormat = SerializationFormats.Json
        };
        var executeStatementAsync = await context.CreateOrReplaceStreamStatement("MessageData").With(creationMetadata).As<MessageData>().ExecuteStatementAsync();
        if (executeStatementAsync.IsSuccessStatusCode)
        {

        }*/

        // adminClient.top

        // consumer.Subscribe("messages");
        // Register new topic/partition
        // Send clear state on WS 
        // Register our table on ksql
        // context.CreateOrReplaceTableStatement(sessionDto.TopicName).As<MessageData>();
        /*await adminClient.CreateTopicsAsync([
            new TopicSpecification { Name = "messages", NumPartitions = 1, ReplicationFactor = 1 }
        ]);*/

        // Start listening and publishing on new 
    }

    private static async Task createTableForTopic()
    {
        string ksqlEndpoint = "http://localhost:8088"; // Change this to your ksqlDB server endpoint
        string ksqlQuery = @"
            CREATE STREAM test_data (
                Row INT,
                Column INT,
                HexColour STRING,
                UserId STRING
            ) WITH (
                KAFKA_TOPIC = 'test_topic',
                VALUE_FORMAT = 'JSON'
            );
        ";

        var ksqlRequest = new
        {
            ksql = ksqlQuery,
            streamsProperties = new { }
        };

        var httpClient = new HttpClient();

        try
        {
            var response = await httpClient.PostAsJsonAsync(ksqlEndpoint, ksqlRequest);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Stream created successfully.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to create stream: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static IAsyncEnumerable<MessageData> GetAllBlocksCurrent(
        ILogger<Program> logger,
        IKSqlDBContext context,
        ClaimsPrincipal claims
    )
    {
        // TODO: use sql like thingy to make call to the latest board state
        return context.CreatePullQuery<MessageData>().GetManyAsync();
        // var observable = context.CreatePushQuery<TestData>().ToObservable();
        /*var observable = context.CreatePullQuery<TestData>()
            .Take(2)
            .ToObservable()
            .Select(data => data)
            .Subscribe(Console.WriteLine,
                error => Console.WriteLine($"Exception: {error.Message}"),
                () => Console.WriteLine("Completed"));
        Console.WriteLine(observable);*/
        // return TypedResults.Json(claims.Claims.Select(claim => $"V - {claim.Value}\t\tT - {claim.Type}"));
    }

    private static async Task<JsonHttpResult<string>> AddBlock(
        ILogger<Program> logger,
        IProducer<Null, MessageData> producer,
        ClaimsPrincipal claims,
        MessageDataDto data
    )
    {
        var userSid = claims.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (PreventionMap.TryGetValue(userSid, out var valueDate))
        {
            if ((DateTime.Now - valueDate).TotalSeconds < 30)
                return TypedResults.Json("You can only send a message every 30 seconds",
                    statusCode: 400);

            PreventionMap[userSid] = DateTime.Now;
        }
        else
            PreventionMap.Add(userSid, DateTime.Now);


        // TODO: figure out if we need to do this as partition or on different topic. 
        await producer.ProduceAsync(_messageGroup, new Message<Null, MessageData>
        {
            Value = new MessageData(data.Column, data.Row, data.HexColour, userSid)
        });
        return TypedResults.Json("success");
    }
}