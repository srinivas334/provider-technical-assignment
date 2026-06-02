-- ============================================================
-- Provider Management System — Seed Data
-- ============================================================
-- This script documents the seed data applied via EF Core's
-- HasData() configuration in AppDbContext.SeedData().
-- The seed covers all three required scenarios:
--   1. Active providers with active licenses
--   2. Active providers with expired licenses
--   3. Soft-deleted provider (audit trail demo)
-- ============================================================

-- Providers
INSERT INTO Providers (ProviderId, ProviderName, County, Status, CreatedDate, IsDeleted, DeletedAt) VALUES
(1,  'Fulton Family Care',       'Fulton',   'Active',   '2023-01-15', 0, NULL),
(2,  'DeKalb Medical Group',     'DeKalb',   'Active',   '2023-03-22', 0, NULL),
(3,  'Gwinnett Health Services', 'Gwinnett', 'Active',   '2023-06-10', 0, NULL),
(4,  'Cobb County Clinics',      'Cobb',     'Inactive', '2022-11-05', 0, NULL),
(5,  'Clayton Care Partners',    'Clayton',  'Pending',  '2024-02-01', 0, NULL),
(6,  'Cherokee Wellness Center', 'Cherokee', 'Active',   '2022-08-19', 0, NULL),
(7,  'Henry County Health',      'Henry',    'Active',   '2023-09-03', 0, NULL),
-- Provider 8 is soft-deleted; it will NOT appear in standard application views
(8,  'Archived Provider Inc',    'Fulton',   'Inactive', '2021-05-14', 1, '2024-01-10'),
(9,  'Douglas Diagnostic Center','Douglas',  'Active',   '2024-04-07', 0, NULL),
(10, 'Forsyth Primary Care',     'Forsyth',  'Pending',  '2024-05-20', 0, NULL);

-- Licenses
INSERT INTO Licenses (LicenseId, ProviderId, LicenseNumber, LicenseStatus, ExpirationDate) VALUES
-- Fulton Family Care: active license + suspended
(1,  1, 'LIC-2023-0001', 'Active',    '2026-01-15'),
(2,  1, 'LIC-2023-0002', 'Suspended', '2025-06-30'),
-- DeKalb Medical Group: one expired, one active (scenario 2: active provider/expired license)
(3,  2, 'LIC-2023-0010', 'Expired',   '2024-03-22'),
(4,  2, 'LIC-2023-0011', 'Active',    '2026-03-22'),
-- Gwinnett: active + expired
(5,  3, 'LIC-2023-0020', 'Active',    '2026-06-10'),
(6,  3, 'LIC-2021-0021', 'Expired',   '2023-06-10'),
-- Cobb: expired only
(7,  4, 'LIC-2022-0030', 'Expired',   '2023-11-05'),
-- Clayton: active
(8,  5, 'LIC-2024-0040', 'Active',    '2027-02-01'),
-- Cherokee: active
(9,  6, 'LIC-2022-0050', 'Active',    '2025-12-31'),
-- Henry: expired (scenario 2: active provider/expired license)
(10, 7, 'LIC-2023-0060', 'Expired',   '2024-09-03'),
-- Archived Provider (soft-deleted): license remains for audit
(11, 8, 'LIC-2021-0070', 'Expired',   '2022-05-14'),
-- Douglas: active
(12, 9, 'LIC-2024-0080', 'Active',    '2027-04-07'),
-- Forsyth: active
(13, 10,'LIC-2024-0090', 'Active',    '2027-05-20');
