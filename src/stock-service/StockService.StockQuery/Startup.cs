using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(StockService.StockQuery.Startup))]

namespace StockService.StockQuery
{
    class Startup : FunctionsStartup
    {
        public Startup()
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables();

            var builtConfig = config.Build();

            config.AddAzureKeyVault(builtConfig["KeyVault:Endpoint"]);

            Configuration = config.Build();
        }

        public IConfiguration Configuration { get; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<StockDbContext>((provider, options) =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlConnection"))
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        }
    }
}
