using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace EfParallelQueriesMsSql;

public class MsSqlTests
{
    private static readonly string databaseName = "EfParallelQueriesMsSql";
    private static readonly string[] usernames = ["john", "jane"];

    private MsSqlContainer msSqlContainer;

    private SampleDbContext CreateDbContext(bool multipleActiveResultSets)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(
            msSqlContainer.GetConnectionString()
        )
        {
            InitialCatalog = databaseName,
            MultipleActiveResultSets = multipleActiveResultSets
        };

        var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>().UseSqlServer(
            msSqlContainer.GetConnectionString()
        );

        return new SampleDbContext(optionsBuilder.Options);
    }

    private async Task SeedUsersAsync()
    {
        using var dbContext = CreateDbContext(multipleActiveResultSets: false);

        await dbContext.Database.EnsureCreatedAsync();

        await dbContext.Users.AddRangeAsync(
            new User("john", "John", "Doe"),
            new User("jane", "Jane", "Doe")
        );

        await dbContext.SaveChangesAsync();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetupAsync()
    {
        msSqlContainer = new MsSqlBuilder().Build();
        await msSqlContainer.StartAsync();

        using (var dbContext = CreateDbContext(multipleActiveResultSets: false))
        {
            await dbContext.Database.MigrateAsync();
        }

        await SeedUsersAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await msSqlContainer.DisposeAsync();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ParallelQueriesWithSingleDbContext(bool multipleActiveResultSets)
    {
        using var dbContext = CreateDbContext(multipleActiveResultSets);

        var action = async () =>
            await Task.WhenAll(
                usernames.Select(username =>
                    dbContext.Users.SingleOrDefaultAsync(user => user.Username == username)
                )
            );

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ParallelQueriesWithMultipleDbContexts(bool multipleActiveResultSets)
    {
        var users = await Task.WhenAll(
            usernames.Select(async username =>
            {
                using var dbContext = CreateDbContext(multipleActiveResultSets);

                return await dbContext.Users.SingleOrDefaultAsync(user =>
                    user.Username == username
                );
            })
        );

        users.Should().HaveCount(2);
        users.Should().NotContainNulls();
    }
}
