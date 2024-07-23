using System.Security.Claims;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using MainEvent.DTO;
using MainEvent.Models;
using Microsoft.AspNetCore.Mvc;

namespace MainEvent.Api;

public static class Endpoints
{
    public static void ResisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetAllBlocksCurrent);
        app.MapPost("/", AddBlock);
        app.MapGet("/admin", StartNewSession);
    }

    private static async Task StartNewSession(
        ILogger<Program> logger,
        IKSqlDBContext context,
        ClaimsPrincipal claims,
        [FromBody] CreateSessionDto sessionDto
    )
    {
        // Register new topic/partition
        // Send clear state on WS 
        // Register our table on ksql
        context.CreateOrReplaceTableStatement(sessionDto.TopicName).As<MessageData>();
        /*await adminClient.CreateTopicsAsync([
            new TopicSpecification { Name = "messages", NumPartitions = 1, ReplicationFactor = 1 }
        ]);*/

        // Start listening and publishing on new 
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

    private static async Task AddBlock(
        ILogger<Program> logger,
        IProducer<Null, MessageData> producer,
        ClaimsPrincipal claims,
        MessageDataDto data
    )
    {
        var userSid = claims.FindFirstValue(ClaimTypes.NameIdentifier)!;
        // TODO: figure out if we need to do this as partition or on different topic. 
        await producer.ProduceAsync("messages", new Message<Null, MessageData>
        {
            Value = new MessageData(data.Column, data.Row, data.HexColour, userSid)
        });
    }
}