using System;
using System.Net.Http;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MongoDbConfig;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

namespace Play.Inventory.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMongo()
                    .AddMongoRepository<InventoryItem>("inventoryitems");

            Random jittrterer = new Random();
            services.AddHttpClient<CatalogClients>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");

            })

            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
               retryCount: 5,
                retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount))
                + TimeSpan.FromMilliseconds(jittrterer.Next(0, 1000)),
             onRetry: (outcome, timeSpan, retryCount) =>
            {
                var servicesProvider = services.BuildServiceProvider();
                servicesProvider.GetService<ILogger<CatalogClients>>()?
                    .LogWarning($"Delaying for {timeSpan.TotalSeconds} seconds,then making retry {retryCount}");
            }
            ))
            
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timeSpan) =>
                {
                    var servicesProvider = services.BuildServiceProvider();
                    servicesProvider.GetService<ILogger<CatalogClients>>()?
                        .LogWarning($"opening the circuit for {timeSpan.TotalSeconds} seconds");
                },
                onReset: () =>
                {
                    var servicesProvider = services.BuildServiceProvider();
                    servicesProvider.GetService<ILogger<CatalogClients>>()?
                        .LogWarning($"Closing the circuite...");

                }

            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
