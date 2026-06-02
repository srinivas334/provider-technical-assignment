-- ============================================================
-- Provider Management System — SQLite Schema
-- ============================================================
-- These scripts document the physical schema created by EF Core
-- migrations. They can be used to recreate the database manually
-- or to understand the design decisions made.
-- ============================================================

-- Providers table
-- IsDeleted + DeletedAt implement soft deletion.
-- A global query filter in AppDbContext ensures all standard
-- application queries automatically exclude IsDeleted = 1.
-- Use IgnoreQueryFilters() for audit/admin access.
CREATE TABLE IF NOT EXISTS "Providers" (
    "ProviderId"   INTEGER NOT NULL CONSTRAINT "PK_Providers" PRIMARY KEY AUTOINCREMENT,
    "ProviderName" TEXT    NOT NULL,
    "County"       TEXT    NOT NULL,
    "Status"       TEXT    NOT NULL DEFAULT 'Pending',
    "CreatedDate"  TEXT    NOT NULL,
    "IsDeleted"    INTEGER NOT NULL DEFAULT 0,
    "DeletedAt"    TEXT    NULL
);

-- Index for soft-delete filter (heavily used by the global query filter)
CREATE INDEX IF NOT EXISTS "IX_Providers_IsDeleted" ON "Providers" ("IsDeleted");

-- Index for searchable ProviderName
CREATE INDEX IF NOT EXISTS "IX_Providers_ProviderName" ON "Providers" ("ProviderName");

-- Licenses table
-- Licenses are physically deleted (no soft-delete requirement on licenses).
-- ExpirationDate drives license validity; LicenseStatus is the business label.
CREATE TABLE IF NOT EXISTS "Licenses" (
    "LicenseId"      INTEGER NOT NULL CONSTRAINT "PK_Licenses" PRIMARY KEY AUTOINCREMENT,
    "ProviderId"     INTEGER NOT NULL,
    "LicenseNumber"  TEXT    NOT NULL,
    "LicenseStatus"  TEXT    NOT NULL DEFAULT 'Active',
    "ExpirationDate" TEXT    NOT NULL,
    CONSTRAINT "FK_Licenses_Providers_ProviderId"
        FOREIGN KEY ("ProviderId") REFERENCES "Providers" ("ProviderId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Licenses_ProviderId" ON "Licenses" ("ProviderId");
