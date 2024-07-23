using MainEvent;
using MainEvent.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using static Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults;

const string defaultAuthPolicy = "default_auth_policy";
const string adminAuthPolicy = "admin_auth_policy";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
    builder =>
    {
        builder.WithOrigins("http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/chatHub")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    }
);

builder.Services.AddAuthorization();

builder.Services.ConfigureOptions<JwtBearerConfigureOptions>();
builder.Services.AddAuthorizationBuilder().AddPolicy(defaultAuthPolicy,
    policy => policy.AddAuthenticationSchemes(AuthenticationScheme).RequireClaim(ClaimTypes.NameIdentifier));

builder.Services.AddAuthorizationBuilder().AddPolicy(adminAuthPolicy,
    policy => policy.AddAuthenticationSchemes(AuthenticationScheme).RequireClaim("cognito:groups", "Admin"));

builder.Services.AddLogging();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");


app.Run();
