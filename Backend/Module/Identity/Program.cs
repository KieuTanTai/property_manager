using System.Text.Json.Serialization;
using Identity.Infrastructure.DIContainer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityRepositoryCollection(builder.Configuration, builder.Environment);
builder.Services.AddIdentityApplicationCollection(builder.Configuration, builder.Environment);
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}


var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();