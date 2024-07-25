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
using MainEvent.Hubs;
using MainEvent.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using static Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults;
using AutoOffsetReset = Confluent.Kafka.AutoOffsetReset;
using EndpointType = ksqlDB.RestApi.Client.KSql.Query.Options.EndpointType;

const string allowSpecificOriginsPolicy = "allow_specific_origins_policy";
const string defaultAuthPolicy = "default_auth_policy";
const string adminAuthPolicy = "admin_auth_policy";

var builder = WebApplication.CreateSlimBuilder(args);
//var corsOrigins = builder.Configuration.GetSection("cors").Get<string[]>() ?? ["http://localhost:5500"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
    builder =>
    {
        builder.WithOrigins(["http://localhost:5500", "https://themainevent.projects.bbdgrad.com"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(AuthenticationScheme)
    .AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // If we have an access token in the request and request is made from the websocket,
                    // We can use it instead of the header (it seems like ws cant have headers)
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for our hub 
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        }
    );


// builder.Services.AddAuthentication(AuthenticationScheme).AddJwtBearer();
builder.Services.ConfigureOptions<JwtBearerConfigureOptions>();
builder.Services.AddAuthorizationBuilder().AddPolicy(defaultAuthPolicy,
    policy => policy.AddAuthenticationSchemes(AuthenticationScheme).RequireClaim(ClaimTypes.NameIdentifier));

builder.Services.AddAuthorizationBuilder().AddPolicy(adminAuthPolicy,
    policy => policy.AddAuthenticationSchemes(AuthenticationScheme).RequireClaim("cognito:groups", "Admin"));

builder.Services.AddSignalR();
builder.Services.AddLogging();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

var ksqlUrl = builder.Configuration.GetSection("KSql").GetValue<string>("url") ?? "";
var contextOptions = new KSqlDbContextOptionsBuilder().UseKSqlDb(ksqlUrl).SetEndpointType(EndpointType.QueryStream)
    .Options;
var kSqlDbContext = new KSqlDBContext(contextOptions);
builder.Services.AddSingleton<IKSqlDBContext, KSqlDBContext>(_ => kSqlDbContext);

kSqlDbContext.CreateOrReplaceTableStatement("messages").With(new EntityCreationMetadata("messages") { Partitions = 1 });

var bootstrapServers = "54.154.112.105:29092";
var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };
builder.Services.AddSingleton<IProducer<Null, MessageData>>(_ =>
    new ProducerBuilder<Null, MessageData>(producerConfig).SetValueSerializer(new JsonSerializable<MessageData>())
        .Build());

var adminConfig = new AdminClientConfig { BootstrapServers = bootstrapServers };
var adminClient = new AdminClientBuilder(adminConfig).Build();
builder.Services.AddSingleton<IAdminClient, IAdminClient>(_ => adminClient);
/*
await adminClient.CreateTopicsAsync([
    new TopicSpecification { Name = "messages", NumPartitions = 1, ReplicationFactor = 1 }
]);
*/

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = bootstrapServers,
    GroupId = "Message consumer group",
    AutoOffsetReset = AutoOffsetReset.Latest
};

var consumer = new ConsumerBuilder<Null, MessageData>(consumerConfig)
    .SetValueDeserializer(new JsonSerializable<MessageData>())
    .Build();
// consumer.Subscribe("messages");
builder.Services.AddSingleton<IConsumer<Null, MessageData>>(_ => consumer);


/*
var earliestConfig = new ConsumerConfig
{
    BootstrapServers = "broker:9092",
    GroupId = "Test",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

var earliest = new ConsumerBuilder<Null, MessageData>(earliestConfig)
    .SetValueDeserializer(new JsonSerializable<MessageData>())
    .Build();
earliest.Subscribe("messages");

builder.Services.AddSingleton<IConsumer<Null, MessageData>>(_ => earliest);
*/


builder.Services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
var app = builder.Build();

var lifetime = app.Lifetime;
var kafkaConsumerService = app.Services.GetRequiredService<IKafkaConsumerService>();

lifetime.ApplicationStarted.Register(() =>
{
    var cancellationTokenSource = new CancellationTokenSource();
    _ = kafkaConsumerService.StartAsync(cancellationTokenSource.Token);
});


// Thanks EBS
app.MapGet("/", () => "Health is ok!").AllowAnonymous();

var group = app.MapGroup("/board").RequireAuthorization("default_auth_policy");
Endpoints.ResisterEndpoints(group);
app.UseCors();
app.UseAuthorization();
app.UseAuthentication();

app.MapHub<ChatHub>("/chatHub");


app.Run();


[JsonSerializable(typeof(MessageData))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;