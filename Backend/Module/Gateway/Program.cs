using System.Text.Json.Serialization;
using Gateway.Middleware;
using Gateway.Services;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ModuleUrls:Identity"] ??= "https://localhost:7048";
builder.Configuration["ModuleUrls:Contract"] ??= "https://localhost:7255";
builder.Configuration["ModuleUrls:Premise"] ??= "https://localhost:7081";
builder.Configuration["ModuleUrls:TicketAndNotification"] ??= "https://localhost:7092";

builder.Services.AddOpenApi();
builder.Services.AddSingleton<ILogPool, LogPool>();
builder.Services.AddProxyMapping(builder.Environment);
builder.Services.AddScoped<ModuleProxyService>();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseWebSockets();
app.UseMiddleware<NotificationHubProxyMiddleware>();
app.MapControllers();
app.Run();