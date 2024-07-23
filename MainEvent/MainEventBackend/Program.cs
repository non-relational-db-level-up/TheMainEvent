using System.Security.Claims;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using MainEvent;
using MainEvent.Api;
using MainEvent.helpers;
using MainEvent.Models;
using static Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults;
using AutoOffsetReset = Confluent.Kafka.AutoOffsetReset;
using EndpointType = ksqlDB.RestApi.Client.KSql.Query.Options.EndpointType;

const string allowSpecificOriginsPolicy = "allow_specific_origins_policy";
const string defaultAuthPolicy = "default_auth_policy";
const string adminAuthPolicy = "admin_auth_policy";


var builder = WebApplication.CreateSlimBuilder(args);
var corsOrigins = builder.Configuration.GetSection("cors").Get<string[]>() ?? [];

builder.Services.AddCors(options => options.AddPolicy(name: allowSpecificOriginsPolicy,
    policy => policy.WithOrigins(corsOrigins)
        .WithHeaders("Content-Type", "Authorization")
        .WithMethods("GET", "POST", "DELETE", "OPTIONS")
        .AllowCredentials()
));
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(AuthenticationScheme).AddJwtBearer();
builder.Services.ConfigureOptions<JwtBearerConfigureOptions>();
builder.Services.AddAuthorizationBuilder().AddPolicy(defaultAuthPolicy,
    policy => policy.AddAuthenticationSchemes(AuthenticationScheme).RequireClaim(ClaimTypes.NameIdentifier));

builder.Services.AddAuthorizationBuilder().AddPolicy(adminAuthPolicy,
    policy => policy.AddAuthenticationSchemes(AuthenticationScheme).RequireClaim("cognito:groups", "Admin"));

builder.Services.AddLogging();


builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

var ksqlUrl = builder.Configuration.GetSection("KSql").GetValue<string>("url") ?? "";
var contextOptions = new KSqlDbContextOptionsBuilder().UseKSqlDb(ksqlUrl).SetEndpointType(EndpointType.QueryStream)
    .Options;
var kSqlDbContext = new KSqlDBContext(contextOptions);
builder.Services.AddSingleton<IKSqlDBContext, KSqlDBContext>(_ => kSqlDbContext);

kSqlDbContext.CreateOrReplaceTableStatement("messages").With(new EntityCreationMetadata("messages") { Partitions = 1 });


var producerConfig = new ProducerConfig { BootstrapServers = "localhost:29092" };
builder.Services.AddSingleton<IProducer<Null, MessageData>>(_ =>
    new ProducerBuilder<Null, MessageData>(producerConfig).SetValueSerializer(new JsonSerializable<MessageData>())
        .Build());

var adminConfig = new AdminClientConfig { BootstrapServers = "localhost:29092" };
var adminClient = new AdminClientBuilder(adminConfig).Build();
// builder.Services.AddSingleton<IAdminClient>(_ => adminClient);

// await adminClient.CreateTopicsAsync([
    // new TopicSpecification { Name = "messages", NumPartitions = 1, ReplicationFactor = 1 }
// ]);

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = "localhost:29092",
    GroupId = "Message consumer group",
    AutoOffsetReset = AutoOffsetReset.Latest
};

var consumer = new ConsumerBuilder<Null, MessageData>(consumerConfig)
    .SetValueDeserializer(new JsonSerializable<MessageData>())
    .Build();
consumer.Subscribe("messages");

// Spin up a thread to handle the consumption of messages.
_ = Task.Run(() =>
    {
        try
        {
            while (true)
            {
                var consumeResult = consumer.Consume();
                //Todo: publish to websocket/Server sent event.
                //Client publish 

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
// Thanks EBS
app.MapGet("/", () => "Health is ok!").AllowAnonymous();

var group = app.MapGroup("/board")
    .RequireCors(allowSpecificOriginsPolicy)
    .RequireAuthorization(defaultAuthPolicy);


Endpoints.ResisterEndpoints(group);
app.UseCors();
app.UseAuthorization();
app.UseAuthentication();

app.Run();


[JsonSerializable(typeof(MessageData))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;