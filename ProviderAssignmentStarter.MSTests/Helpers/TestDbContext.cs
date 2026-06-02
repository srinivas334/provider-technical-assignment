using Microsoft.EntityFrameworkCore;
using ProviderAssignmentStarter.Data;

namespace ProviderAssignmentStarter.MSTests.Helpers;

public class TestAppDbContext : AppDbContext
{
    public TestAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void SeedTestData(ModelBuilder modelBuilder) { }
}

public static class TestDbContext
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new TestAppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
