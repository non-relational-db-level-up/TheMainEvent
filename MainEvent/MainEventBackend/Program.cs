using System.Text.Json.Serialization;
using Confluent.Kafka;
using MainEvent.Api;
using MainEvent.helpers;
using MainEvent.Helpers.Cognito;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("OurCors", corsBuilder =>
    {
        corsBuilder.WithOrigins(["http://localhost:4200", "https://"])
            .WithHeaders(["Content-Type", "Authorization"])
            .WithMethods([HttpMethods.Get, HttpMethods.Post, HttpMethods.Delete, HttpMethods.Options]).Build();
    });
});

builder.Services.AddAuthorization();
/*
 // TODO auth ->
 builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
            {
                var json = new HttpClient().GetStringAsync(parameters.ValidIssuer + "/.well-known/jwks.json").Result;
                var keys = JsonSerializer.Deserialize<JsonWebKeySet>(json)?.Keys;
                return keys!;
            },

            ValidIssuer = "https://cognito-idp.eu-west-1.amazonaws.com/eu-west-1_28ckopm51",
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidAudience = "7g2q9pb8e9ro0bb1hpp8vc0i4n",
            ValidateAudience = false
        };
    });*/

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));


var producerConfig = new ProducerConfig { BootstrapServers = "localhost:29092" };
builder.Services.AddSingleton<IProducer<Null, TestData>>(_ =>
    new ProducerBuilder<Null, TestData>(producerConfig).SetValueSerializer(new JsonSerializable<TestData>()).Build());

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = "localhost:29092",
    GroupId = "kafka-dotnet-getting-started",
    AutoOffsetReset = AutoOffsetReset.Earliest
};
var consumer = new ConsumerBuilder<Null, TestData>(consumerConfig)
    .SetValueDeserializer(new JsonSerializable<TestData>())
    .Build();
consumer.Subscribe("messages");

//TODO this feels like it could be better 
Task.Run(() =>
    {
        try
        {
            while (true)
            {
                Console.WriteLine("We are consuming ");
                var consumeResult = consumer.Consume();

                //Todo: publish to websocket/Server sent event.
                Console.WriteLine(
                    $"Consumed event from topic messages: key = {consumeResult.Message.Key,-10} value = {consumeResult.Message.Value}");
            }
        }
        finally
        {
            consumer.Close();
        }
    }
);

var app = builder.Build();


var todosApi = app.MapGroup("/messages");
todosApi.MapPost("/", () => { });
todosApi.MapGet("/all", () => { });

app.UseCors("OurCors");
app.UseAuthorization();
app.UseAuthentication();
app.UseMiddleware<CognitoMiddleware>();
app.MapGet("/", () => "Health is ok!").AllowAnonymous();
var group = app.MapGroup("/");
Endpoints.ResisterEndpoints(group);

app.Run();


[JsonSerializable(typeof(TestData))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;

record TestData(int PosX, int PosY, int R, int G, int B);