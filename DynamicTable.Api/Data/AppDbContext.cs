using Microsoft.EntityFrameworkCore;

namespace DynamicTable.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(DbConstants.ConnectionString);

        base.OnConfiguring(optionsBuilder);
    }
}