using System.Text.Json.Serialization;
using TicketAndNotification.Infrastructure.DIContainer;
using TicketAndNotification.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                     ["http://localhost:8443", "https://localhost:8443"];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddTicketAndNotificationRepositoryCollection(
    builder.Configuration, builder.Environment);
builder.Services.AddTicketAndNotificationApplicationCollection();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>(NotificationHub.Route).RequireAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
