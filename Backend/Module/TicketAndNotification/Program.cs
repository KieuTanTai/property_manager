using System.Text.Json.Serialization;
using TicketAndNotification.Infrastructure.DIContainer;
using TicketAndNotification.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
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
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>(NotificationHub.Route).RequireAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();