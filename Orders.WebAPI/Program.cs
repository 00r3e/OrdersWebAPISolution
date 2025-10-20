using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Orders.WebAPI.ApplicationDbContext;
using Orders.WebAPI.StartupExtensions;
using Repositories;
using RepositoryContracts;
using Serilog;
using ServiceContracts.ICountriesServices;
using ServiceContracts.ICustomersServices;
using ServiceContracts.IOrderItemReviewsServices;
using ServiceContracts.IOrderItemsServices;
using ServiceContracts.IOrdersServices;
using Services.CountriesServices;
using Services.CustomersServices;
using Services.OrderItemsReviewsServices;
using Services.OrderItemsServices;
using Services.OrdersServices;
using UnitOfWork;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider service, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration) // Read configuration settings from build-in IConfiguration
        .ReadFrom.Services(service); // Read app's services 
});

// Add services to the container.
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHsts(); // enable Https protocol

app.UseHttpsRedirection();

app.UseSwagger(); //creates andpoint for swagger.json
app.UseSwaggerUI(); //creates UI


app.UseSerilogRequestLogging();

app.UseHttpLogging();

app.UseAuthorization();

app.MapControllers();

app.Run();
