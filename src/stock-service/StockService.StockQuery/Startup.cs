using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(StockService.StockQuery.Startup))]

namespace StockService.StockQuery
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<StockDbContext>((provider, options) =>
            {
                options.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnection", EnvironmentVariableTarget.Process))
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        }
    }
}
