using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfParallelQueriesMsSql;

internal class SampleDbContextFactory : IDesignTimeDbContextFactory<SampleDbContext>
{
    public SampleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>();
        optionsBuilder.UseSqlServer("");

        return new SampleDbContext(optionsBuilder.Options);
    }
}
