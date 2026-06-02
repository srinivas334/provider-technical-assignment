using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProviderAssignmentStarter.Models;
using ProviderAssignmentStarter.MSTests.Helpers;
using ProviderAssignmentStarter.Services;

namespace ProviderAssignmentStarter.MSTests.Services;

[TestClass]
public class LicenseServiceTests
{
    private static (LicenseService svc, IAuditService audit) Build()
    {
        var db    = TestDbContext.Create();
        var audit = Substitute.For<IAuditService>();
        db.Providers.Add(new Provider
        {
            ProviderId = 1, ProviderName = "Test Provider",
            County = "Fulton", Status = "Active"
        });
        db.SaveChanges();
        return (new LicenseService(db, audit, Microsoft.Extensions.Logging.Abstractions.NullLogger<LicenseService>.Instance), audit);
    }

    [TestMethod]
    public async Task IsDuplicateLicenseNumberAsync_ReturnsFalse_WhenNumberIsUnique()
    {
        var (svc, _) = Build();
        var result = await svc.IsDuplicateLicenseNumberAsync("NEW-LIC-001");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsDuplicateLicenseNumberAsync_ReturnsTrue_WhenNumberAlreadyExists()
    {
        var (svc, _) = Build();
        await svc.CreateAsync(new License
        {
            ProviderId = 1, LicenseNumber = "DUP-001",
            LicenseStatus = "Active", ExpirationDate = DateTime.UtcNow.AddYears(1)
        });

        var result = await svc.IsDuplicateLicenseNumberAsync("DUP-001");
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CreateAsync_PersistsLicenseAndWritesAuditLog()
    {
        var (svc, audit) = Build();

        var license = await svc.CreateAsync(new License
        {
            ProviderId     = 1,
            LicenseNumber  = "LIC-CREATE-01",
            LicenseStatus  = "Active",
            ExpirationDate = DateTime.UtcNow.AddYears(2)
        });

        Assert.IsTrue(license.LicenseId > 0);
        await audit.Received(1).LogAsync(
            "License", license.LicenseId, "Create",
            Arg.Any<string>(), Arg.Any<string>());
    }
}
