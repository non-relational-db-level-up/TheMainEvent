using System.Reactive.Linq;
using System.Security.Claims;
using Confluent.Kafka;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MainEvent.Api;

public static class Endpoints
{
    public static void ResisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetAllBlocks1);
        app.MapPost("/", AddBlock);
    }

    private static async Task<string> GetAllBlocks(
        ILogger<Program> logger
    )
    {
        return "Cool stuff";
    }

    [Authorize]
    private static async Task<JsonHttpResult<IEnumerable<string>>> GetAllBlocks1(
        ILogger<Program> logger,
        IKSqlDBContext context,
        ClaimsPrincipal claims)
    {
        // TODO: use sql like thingy to make call to the latest board state

        // var observable = context.CreatePushQuery<TestData>().ToObservable();
        var observable = context.CreatePushQuery<TestData>()
            .Take(2)
            .ToObservable()
            .Select(data => data)
            .ToList();

        Console.WriteLine(observable);
        return TypedResults.Json(claims.Claims.Select(claim => $"V - {claim.Value}\t\tT - {claim.Type}"));
    }

    private static async Task AddBlock(
        ILogger<Program> logger,
        IProducer<Null, TestData> producer
    )
    {
        // Do timer check 
        // TODO: some sort of abstraction for this.
        // TODO: Port this to lambda
        var result = await producer.ProduceAsync("messages", new Message<Null, TestData>
        {
            Value = new TestData(2, 1, 10, 20, 30)
        });
        Console.WriteLine(result);
        throw new NotImplementedException();
    }
}