using System.Text.Json.Serialization;
using MainEvent;
using MainEvent.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateSlimBuilder(args);
var corsOrigins = builder.Configuration.GetSection("cors").Get<string[]>() ?? [];
const string allowSpecificOriginsPolicy = "allow_specific_origins_policy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOriginsPolicy,
        policy => policy.WithOrigins(corsOrigins)
            .WithHeaders("Content-Type", "Authorization")
            .WithMethods("GET", "POST", "DELETE", "OPTIONS")
            .AllowCredentials()
    );
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.ConfigureOptions<JwtBearerConfigureOptions>();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));


/*var producerConfig = new ProducerConfig { BootstrapServers = "localhost:29092" };
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
    .Build();*/
// consumer.Subscribe("messages");

//TODO this feels like it could be better 
/*Task.Run(() =>
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
);*/

var app = builder.Build();
// Thanks EBS
app.MapGet("/", () => "Health is ok!").AllowAnonymous();


app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.UseAuthentication();

var group = app.MapGroup("/board").RequireCors(allowSpecificOriginsPolicy).RequireAuthorization();
Endpoints.ResisterEndpoints(group);
app.Run();


[JsonSerializable(typeof(TestData))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;

record TestData(int PosX, int PosY, int R, int G, int B);