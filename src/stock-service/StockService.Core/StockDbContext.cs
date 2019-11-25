using System;

using Microsoft.EntityFrameworkCore;

using StockService.Entities;

namespace StockService
{
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options)
          :base(options)
        { }

        public DbSet<StockEntity> Stocks { get; set; }
    }
}
