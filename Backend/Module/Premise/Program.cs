using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Premise.Application;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Infrastructure.Repository;
using Premise.Infrastructure.Repository.PremiseRepository;
using Premise.Interfaces.IRepository;
using Premise.Interfaces.BusinessType;
using Premise.Interfaces.Location;
using Premise.Interfaces.Premise;
using Premise.Interfaces.Product;
using Shared.Interfaces;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var connectionString = builder.Configuration.GetConnectionString("Premise")
                       ?? builder.Configuration.GetConnectionString("IdentityTest")
                       ?? throw new InvalidOperationException(
                           "Connection string 'Premise' (or 'IdentityTest') was not found.");

builder.Services.AddDbContext<PremiseDbContext>(options => options.UseMySQL(connectionString));
builder.Services.AddSingleton<ILogPool, LogPool>();
builder.Services.AddScoped<IUnitOfWork, EfPremiseUnitOfWork>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<LocationApplication>();
builder.Services.AddScoped<IPremiseRepository, PremiseRepository>();
builder.Services.AddScoped<IPremiseMediaRepository, PremiseMediaRepository>();
builder.Services.AddScoped<IBusinessTypeRepository, BusinessTypeRepository>();
builder.Services.AddScoped<IPremiseBusinessTypeRepository, PremiseBusinessTypeRepository>();
builder.Services.AddScoped<PremiseApplication>();
builder.Services.AddScoped<IWhitelistProductRepository, WhitelistProductRepository>();
builder.Services.AddScoped<IProductBusinessTypeRepository, ProductBusinessTypeRepository>();
builder.Services.AddScoped<ProductApplication>();

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
