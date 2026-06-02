# Provider Management System



\---

## How to Run the Application

### Prerequisites

* .NET 8 SDK
* Node.js 18+ and npm (for the React dashboard)

### 1 — Run the MVC application

```bash
dotnet restore
dotnet run
```

Available at **http://localhost:5236**. The SQLite database (`Data/providers.db`) is created and seeded automatically on first run.

### 2 — Run the React Dashboard (optional)

```bash
cd ClientApp
npm install
npm run dev
```

Dashboard available at **http://localhost:5173**. The Vite dev server proxies `/api/\*` to the .NET backend — both processes must be running simultaneously. A **Dashboard ↗** link in the MVC nav opens it in a new tab.

\---

## Project Structure

```
ProviderAssignmentStarter/
├── Controllers/
│   ├── ProvidersController.cs    # CRUD + soft delete
│   ├── LicensesController.cs     # License CRUD per provider
│   ├── ReportsController.cs      # Required data scenario reports
│   └── DashboardApiController.cs # GET /api/dashboard — feeds React app
├── Data/
│   ├── AppDbContext.cs            # EF Core context with global query filter
│   ├── Migrations/                # EF Core migrations (hand-authored)
│   ├── schema.sql                 # Reference: physical schema documentation
│   ├── views.sql                  # SQLite views for the 3 required scenarios
│   └── seed.sql                   # Reference: seed data documentation
├── Models/
│   ├── Provider.cs
│   ├── License.cs
│   └── ViewModels/                # Form/display/dashboard view models
├── Services/
│   ├── IProviderService.cs
│   ├── ProviderService.cs
│   ├── ILicenseService.cs
│   └── LicenseService.cs
├── Views/
│   ├── Providers/                 # Index, Details, Create, Edit, Delete
│   ├── Licenses/                  # Create, Edit, Delete
│   └── Reports/                   # Index (3 required scenarios)
└── ClientApp/                     # React dashboard (Vite + TypeScript + Tailwind)
    ├── src/
    │   ├── App.tsx                # Root component, data fetch
    │   ├── types.ts               # Shared TypeScript interfaces
    │   └── components/
    │       ├── Header.tsx         #  state gov branding
    │       ├── SummaryCards.tsx   # 6 KPI stat cards
    │       ├── ProviderStatusChart.tsx   # Donut chart
    │       ├── LicenseStatusChart.tsx    # Bar chart
    │       ├── LicensesPerProviderChart.tsx  # Stacked bar
    │       └── ExpiringLicensesTable.tsx     # Expiry alert table
    └── vite.config.ts             # Proxies /api → :5236
```

\---

## Database Design Decisions

### Technology

* **SQLite** via Entity Framework Core 8 (Microsoft.EntityFrameworkCore.Sqlite)
* Database file: `Data/providers.db` (path configurable in `appsettings.json`)

### Schema

**Providers table**

|Column|Type|Notes|
|-|-|-|
|ProviderId|INTEGER|Primary key, autoincrement|
|ProviderName|TEXT|Required, max 200 chars, indexed|
|County|TEXT|Required, max 100 chars|
|Status|TEXT|Active / Inactive / Pending|
|CreatedDate|TEXT|Set on creation, UTC|
|IsDeleted|INTEGER|0 = active, 1 = soft-deleted; indexed|
|DeletedAt|TEXT|Nullable; set when soft-deleted, UTC|

**Licenses table**

|Column|Type|Notes|
|-|-|-|
|LicenseId|INTEGER|Primary key, autoincrement|
|ProviderId|INTEGER|Foreign key → Providers (cascade delete)|
|LicenseNumber|TEXT|Business identifier|
|LicenseStatus|TEXT|Active / Expired / Suspended|
|ExpirationDate|TEXT|Drives validity checks|

**Indexes**

* `IX\_Providers\_IsDeleted` — supports the global soft-delete filter (every standard query hits this)
* `IX\_Providers\_ProviderName` — supports name search
* `IX\_Licenses\_ProviderId` — join performance

**SQLite Views** (`Data/views.sql`)

Three views are provided for DBA/support use and document the required data scenarios:

* `vw\_ActiveProviders` — mirrors the EF Core global query filter
* `vw\_ActiveProvidersWithActiveLicenses` — Scenario 1
* `vw\_ActiveProvidersWithExpiredLicenses` — Scenario 2
* `vw\_SoftDeletedProviders` — Scenario 3 / audit trail

\---

## Soft Deletion Implementation

### Design Choice: `IsDeleted` + `DeletedAt` columns on the `Providers` table

I chose two columns rather than one:

* `IsDeleted` (bool) — the filter predicate; a single-byte integer that SQLite indexes efficiently.
* `DeletedAt` (DateTime?) — the audit timestamp; tells operations staff *when* the record was deleted without querying change logs.

### Enforcement: EF Core Global Query Filter

```csharp
modelBuilder.Entity<Provider>()
    .HasQueryFilter(p => !p.IsDeleted);
```

This filter is registered once in `AppDbContext.OnModelCreating` and is **automatically applied to every LINQ query** that touches `Providers`, including queries via navigation properties from `Licenses`. A developer cannot accidentally forget to filter — they would have to explicitly opt out.

### Audit / Admin Access

Any query that legitimately needs to see deleted records uses `.IgnoreQueryFilters()`:

```csharp
// ProviderService.GetAllIncludingDeletedAsync()
return await \_db.Providers
    .IgnoreQueryFilters()
    .Include(p => p.Licenses)
    .ToListAsync();
```

This is used by the Reports page (Scenario 3 — soft-deleted audit trail).

### What happens on soft delete

```csharp
provider.IsDeleted = true;
provider.DeletedAt = DateTime.UtcNow;
await \_db.SaveChangesAsync();
```

The row is never `DELETE`d. After this call, the provider disappears from all standard application queries automatically via the global filter. Its licenses remain in the database and are also excluded from view (since EF's filter propagates through the navigation property).

### Why not a separate `DeletedProviders` archive table?

An archive table is a valid pattern but introduces two sources of truth. With `IsDeleted` + global query filter, the data is always consistent — there is no risk of a row existing in one place but not the other. The filter is also zero-cost for standard queries when an index on `IsDeleted` is present.

\---

## Required Data Scenarios

All three are accessible via the **Reports** page (`/Reports`).

|Scenario|How Implemented|
|-|-|
|Active providers with active licenses|`ProviderService.GetActiveWithActiveLicensesAsync()` — LINQ filter + `vw\_ActiveProvidersWithActiveLicenses` view|
|Active providers with expired licenses|`ProviderService.GetActiveWithExpiredLicensesAsync()` — checks both `LicenseStatus == "Expired"` and `ExpirationDate < now`|
|Soft-deleted providers excluded from standard results|EF Core global query filter on `IsDeleted`|

The service layer owns this logic — the Reports controller does not contain any query predicates.

\---

## Trade-offs and Assumptions

|Decision|Trade-off|
|-|-|
|EF Core global query filter for soft delete|Enforcement is automatic and fail-safe. Trade-off: developers must know to use `IgnoreQueryFilters()` for admin/audit work.|
|Service layer between controller and DbContext|Keeps controllers thin and logic testable without HTTP context. Trade-off: extra indirection for a small project.|
|`DateTime.UtcNow` throughout|Avoids timezone ambiguity in a multi-environment deployment. Trade-off: UI displays UTC, which is not user-friendly without a timezone conversion layer.|
|Seed data via EF Core `HasData()`|Migrations carry the seed atomically. Trade-off: changing seed data requires a new migration.|
|Licenses use physical delete|The assignment only specifies soft-delete for Providers. Licenses are subordinate records; their removal is operationally intentional.|
|`LicenseStatus` field + `ExpirationDate` column|Both signals used for expiry checks — a license can be marked Expired by status OR be past its expiration date. This mirrors real-world scenarios where the status field may lag the calendar date.|



