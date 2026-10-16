using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Premise.Application;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Infrastructure.Repository;
using Premise.Infrastructure.Repository.PremiseRepository;
using Premise.Interfaces.IRepository.BusinessType;
using Premise.Interfaces.IRepository.Location;
using Premise.Interfaces.IRepository.Premise;
using Premise.Interfaces.IRepository.Product;
using Shared.Interfaces;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var connectionString = builder.Configuration.GetConnectionString("Premise")
                       ?? builder.Configuration.GetConnectionString("PremiseTest")
                       ?? throw new InvalidOperationException(
                           "Connection string 'Premise' was not found.");

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
