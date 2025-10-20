using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.WebAPI.ApplicationDbContext;
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

namespace Orders.WebAPI.StartupExtensions
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddControllers();



            services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("Default")));

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderItemReviewRepository, OrderItemReviewRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();

            services.AddScoped<IOrdersAdderService, OrdersAdderService>();
            services.AddScoped<IOrdersGetterService, OrdersGetterService>();
            services.AddScoped<IOrdersUpdaterService, OrdersUpdaterService>();
            services.AddScoped<IOrdersDeleterService, OrdersDeleterService>();
            services.AddScoped<IOrdersBatchService, OrdersBatchService>();

            services.AddScoped<IOrderItemsAdderService, OrderItemsAdderService>();
            services.AddScoped<IOrderItemsGetterService, OrderItemsGetterService>();
            services.AddScoped<IOrderItemsUpdaterService, OrderItemsUpdaterService>();
            services.AddScoped<IOrderItemsDeleterService, OrderItemsDeleterService>();
            services.AddScoped<IOrderItemsBatchService, OrderItemsBatchService>();

            services.AddScoped<IOrderItemReviewsAdderService, OrderItemReviewsAdderService>();
            services.AddScoped<IOrderItemReviewsGetterService, OrderItemReviewsGetterService>();
            services.AddScoped<IOrderItemReviewsUpdaterService, OrderItemReviewsUpdaterService>();
            services.AddScoped<IOrderItemReviewsDeleterService, OrderItemReviewsDeleterService>();

            services.AddScoped<ICountriesAdderService, CountriesAdderService>();
            services.AddScoped<ICountriesGetterService, CountriesGetterService>();
            services.AddScoped<ICountriesUpdaterService, CountriesUpdaterService>();
            services.AddScoped<ICountriesDeleterService, CountriesDeleterService>();

            services.AddScoped<ICustomersAdderService, CustomersAdderService>();
            services.AddScoped<ICustomersGetterService, CustomersGetterService>();
            services.AddScoped<ICustomersUpdaterService, CustomersUpdaterService>();
            services.AddScoped<ICustomersDeleterService, CustomersDeleterService>();

            services.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();



            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |
                Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
            });

            //Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;

        }
    }
}
