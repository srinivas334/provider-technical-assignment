using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProviderAssignmentStarter.Models;
using ProviderAssignmentStarter.MSTests.Helpers;
using ProviderAssignmentStarter.Services;

namespace ProviderAssignmentStarter.MSTests.Services;

[TestClass]
public class ProviderServiceTests
{
    private static (ProviderService svc, IAuditService audit) Build()
    {
        var db    = TestDbContext.Create();
        var audit = Substitute.For<IAuditService>();
        return (new ProviderService(db, audit, Microsoft.Extensions.Logging.Abstractions.NullLogger<ProviderService>.Instance), audit);
    }

    [TestMethod]
    public async Task GetAllAsync_ExcludesSoftDeletedProviders()
    {
        var db    = TestDbContext.Create();
        var audit = Substitute.For<IAuditService>();
        var svc   = new ProviderService(db, audit, Microsoft.Extensions.Logging.Abstractions.NullLogger<ProviderService>.Instance);

        db.Providers.Add(new Provider { ProviderName = "Active Co",  County = "Fulton", Status = "Active",   IsDeleted = false });
        db.Providers.Add(new Provider { ProviderName = "Deleted Co", County = "Cobb",   Status = "Inactive", IsDeleted = true, DeletedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var result = (await svc.GetAllAsync()).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Active Co", result[0].ProviderName);
    }

    [TestMethod]
    public async Task FindByNameAndCountyAsync_ReturnsDuplicate_WhenNameAndCountyMatch()
    {
        var (svc, _) = Build();
        await svc.CreateAsync(new Provider { ProviderName = "Test Clinic", County = "Fulton", Status = "Active" });

        var found = await svc.FindByNameAndCountyAsync("Test Clinic", "Fulton");

        Assert.IsNotNull(found);
        Assert.AreEqual("Test Clinic", found.ProviderName);
        Assert.AreEqual("Fulton", found.County);
    }

    [TestMethod]
    public async Task FindByNameAndCountyAsync_ReturnsNull_WhenCountyDiffers()
    {
        var (svc, _) = Build();
        await svc.CreateAsync(new Provider { ProviderName = "Test Clinic", County = "Fulton", Status = "Active" });

        var found = await svc.FindByNameAndCountyAsync("Test Clinic", "Cobb");

        Assert.IsNull(found);
    }

    [TestMethod]
    public async Task FindByNameAndCountyAsync_ExcludesSpecifiedProvider()
    {
        var (svc, _) = Build();
        var provider = await svc.CreateAsync(new Provider { ProviderName = "Test Clinic", County = "Fulton", Status = "Active" });

        var found = await svc.FindByNameAndCountyAsync("Test Clinic", "Fulton", excludeProviderId: provider.ProviderId);

        Assert.IsNull(found);
    }

    [TestMethod]
    public async Task CreateAsync_PersistsProviderAndWritesAuditLog()
    {
        var (svc, audit) = Build();

        var provider = await svc.CreateAsync(new Provider
        {
            ProviderName = "New Provider",
            County       = "DeKalb",
            Status       = "Pending"
        });

        Assert.IsTrue(provider.ProviderId > 0);
        Assert.IsFalse(provider.IsDeleted);
        Assert.AreNotEqual(default, provider.CreatedDate);

        await audit.Received(1).LogAsync(
            "Provider", provider.ProviderId, "Create",
            Arg.Any<string>(), Arg.Any<string>());
    }
}
