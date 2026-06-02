-- ============================================================
-- Provider Management System — SQLite Views
-- ============================================================
-- These views provide reusable, named query patterns for the
-- three required data scenarios. In this application they are
-- supplementary to EF Core queries (which use the global query
-- filter). Views are useful for raw SQL tools, reporting, and
-- DBA/support queries.
-- ============================================================

-- Standard active provider listing (mirrors the EF Core global query filter)
DROP VIEW IF EXISTS vw_ActiveProviders;
CREATE VIEW vw_ActiveProviders AS
SELECT
    ProviderId,
    ProviderName,
    County,
    Status,
    CreatedDate
FROM Providers
WHERE IsDeleted = 0;

-- ============================================================
-- Scenario 1: Active providers that have at least one active license
-- ============================================================
DROP VIEW IF EXISTS vw_ActiveProvidersWithActiveLicenses;
CREATE VIEW vw_ActiveProvidersWithActiveLicenses AS
SELECT DISTINCT
    p.ProviderId,
    p.ProviderName,
    p.County,
    p.Status,
    l.LicenseId,
    l.LicenseNumber,
    l.LicenseStatus,
    l.ExpirationDate
FROM Providers p
INNER JOIN Licenses l ON p.ProviderId = l.ProviderId
WHERE p.IsDeleted = 0
  AND p.Status    = 'Active'
  AND l.LicenseStatus = 'Active';

-- ============================================================
-- Scenario 2: Active providers that have at least one expired license
-- Uses both LicenseStatus field AND ExpirationDate as signals
-- ============================================================
DROP VIEW IF EXISTS vw_ActiveProvidersWithExpiredLicenses;
CREATE VIEW vw_ActiveProvidersWithExpiredLicenses AS
SELECT DISTINCT
    p.ProviderId,
    p.ProviderName,
    p.County,
    p.Status,
    l.LicenseId,
    l.LicenseNumber,
    l.LicenseStatus,
    l.ExpirationDate
FROM Providers p
INNER JOIN Licenses l ON p.ProviderId = l.ProviderId
WHERE p.IsDeleted  = 0
  AND p.Status     = 'Active'
  AND (l.LicenseStatus = 'Expired' OR l.ExpirationDate < DATE('now'));

-- ============================================================
-- Scenario 3: Soft-deleted providers (audit / support use)
-- Bypasses the standard active-only filter intentionally
-- ============================================================
DROP VIEW IF EXISTS vw_SoftDeletedProviders;
CREATE VIEW vw_SoftDeletedProviders AS
SELECT
    ProviderId,
    ProviderName,
    County,
    Status,
    CreatedDate,
    DeletedAt
FROM Providers
WHERE IsDeleted = 1;
