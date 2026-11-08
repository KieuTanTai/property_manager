using System.Text.Json.Serialization;
using Gateway.Infrastructure.DIContainer;
using Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProxyMapping(builder.Environment);
builder.Services.AddGatewayCollection(builder.Configuration, builder.Environment);
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<RequestDeadlineMiddleware>();
app.UseHttpsRedirection();
app.UseMiddleware<IdentityTokenAuthorizationMiddleware>();
app.MapControllers();
app.Run();
