using Microsoft.EntityFrameworkCore;

namespace EfParallelQueriesMsSql;

internal class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}
