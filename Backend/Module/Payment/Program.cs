using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Payment.Infrastructure.DIContainer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddPaymentRepositoryCollection(builder.Configuration, builder.Environment);
builder.Services.AddPaymentApplicationCollection(builder.Configuration);

// The module is normally reached through a tunnel or reverse proxy (VNPAY IPN, docker).
// Without this, [RequireHttps] would reject requests the proxy forwards as plain http,
// because the client side is not loopback and its headers would be ignored.
builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

app.UseForwardedHeaders();

// No UseHttpsRedirection on purpose: the VNPAY IPN/Callback endpoints are reached through a
// TLS-terminating tunnel (https://public -> http://localhost:7093). Redirecting those requests
// would break IPN processing. [RequireHttps] is kept on the client-facing endpoints instead.
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
