using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskFlow.Api.Data;

/// <summary>
/// Used by `dotnet ef` at design time so migrations can be generated without
/// booting the full web host (and without a live database connection).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=taskflow;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options);
    }
}
