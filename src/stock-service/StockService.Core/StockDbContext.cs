using System;

using Microsoft.EntityFrameworkCore;

using StockService.Entities;

namespace StockService
{
    public class StockDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnection", EnvironmentVariableTarget.Process))
                          .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        public DbSet<StockEntity> Stocks { get; set; }
    }
}
