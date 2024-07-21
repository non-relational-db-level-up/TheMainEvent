using Confluent.Kafka;
using MainEvent.Helpers.Cognito;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MainEvent.Api;

public static class Endpoints
{
    public static void ResisterEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/board");
        group.MapGet("/", GetAllBlocks);
        group.MapPost("/", AddBlock);
    }

    private static Task GetAllBlocks(
        ILogger<Program> logger,
        ICognitoService cognito)
    {
        // TODO: use sql like thingy to make call to the latest board state 
        throw new NotImplementedException();
    }

    private static async Task AddBlock(
        ILogger<Program> logger,
        IProducer<Null, TestData> producer,
        ICognitoService cognito
    )
    {
        // Do timer check 
        // TODO: some sort of abstraction for this.
        var result = await producer.ProduceAsync("messages", new Message<Null, TestData>
        {
            Value = new TestData(2, 1, 10, 20, 30)
        });
        Console.WriteLine(result);
        throw new NotImplementedException();
    }
}