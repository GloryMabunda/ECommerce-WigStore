# M1 · Foundation & Database Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stand up the Inches Wig Store solution, CI, and the complete relational data model (with constraints proven by tests against real SQL Server), so M2 onward builds on a tested schema.

**Architecture:** One ASP.NET Core MVC project (`src/InchesWigStore.Web`) holding entities, EF Core configuration and migrations, plus an xUnit project (`tests/InchesWigStore.Tests`) whose integration tests run against a real SQL Server (LocalDB locally, a service container in CI). Constraints that protect business rules (unique SKU, stock never negative, snapshots, idempotent notifications) live in the database and each is pinned by a test. No features beyond schema, seeding and a health check.

**Tech Stack:** C#, .NET 10 LTS (SDK 10.0.404 installed), ASP.NET Core MVC, ASP.NET Core Identity (`IdentityDbContext`), EF Core + SQL Server provider, xUnit, GitHub Actions.

**Spec:** [`docs/specification.md`](../specification.md) (v0.1) and [`docs/requirements.md`](../requirements.md) (v0.1). M1 covers: FR-VAR-1, FR-VAR-2, FR-VAR-5, NFR-QUA-4 to NFR-QUA-6, the data model (spec §7), plus the schema-level halves of FR-ORD-5/6/7, FR-PAY-10, FR-VAR-6, FR-ACC-8 and NFR-OPS-2.

## Global Constraints

- Target `net10.0`; `global.json` pins SDK `10.0.100` with `rollForward: latestFeature`.
- All money is whole cents in a `long` property named `*Cents`, stored as `bigint`, ZAR only (BR-1). No `float`, `double` or `decimal` for money.
- SKUs are unique (BR-2, FR-VAR-2). Stock quantity can never be negative, enforced by a database check constraint (FR-VAR-5, BR-5).
- Order items store their own copies of product name, options, SKU, quantity and price paid; the order stores the delivery address used (BR-4).
- Every stock change is recorded with old quantity, new quantity, reason, time and actor (BR-6).
- Order status and payment status are separate and each change is kept in history (BR-8).
- Payment notifications carry an identifier stored uniquely, for idempotency (BR-13).
- No card numbers or secrets in the schema or in Git (FR-PAY-12, NFR-SEC-6). Admin credentials come from environment variables only (FR-ACC-8).
- Timestamps are `DateTimeOffset` in UTC. Primary keys are `int` identity, except Identity users and roles which use `Guid`.
- Every change goes through an issue and a pull request; the `main` branch requires CI to pass (NFR-QUA-3, NFR-QUA-5).
- Namespace root `InchesWigStore.Web`; entities in `Models/`, EF configuration in `Data/Configurations/` (one `IEntityTypeConfiguration<T>` per entity), per the README structure.

## Review Focus

1. **Deleting something an order depends on.** Deleting a variant, product or customer that appears in an order must be refused by the database (deactivate or anonymise instead). Pinned in Task 4.
2. **SKU that differs only by case.** `wig-001` and `WIG-001` must count as the same SKU (SQL Server's default collation is case-insensitive). Pinned in Task 2.
3. **Two variants with the same option combination.** A product cannot have two 22-inch wavy black variants, including accessories whose unused options are all null. Pinned in Task 2.
4. **Two admins adjusting the same variant's stock at once.** The second save must fail with a concurrency error, not silently overwrite. Pinned in Task 2.
5. **Negative or absurd values.** Negative price, negative stock, an order whose total does not equal subtotal plus delivery fee, and a zero or negative quantity must be refused by the database. Pinned in Tasks 2 and 4.

---

## File Structure

```
global.json
Directory.Build.props
.editorconfig
.gitignore                                   (modify/create: .NET ignores)
InchesWigStore.sln
.github/workflows/ci.yml
src/InchesWigStore.Web/
  InchesWigStore.Web.csproj
  Program.cs
  appsettings.json
  Models/            ApplicationUser, Address, Category, Product, ProductVariant,
                     ProductImage, TryOnAsset, Cart, CartItem, Order, OrderItem,
                     DeliveryAddressSnapshot, OrderStatusHistory, Payment,
                     PaymentNotification, StockMovement, Enums.cs
  Data/              AppDbContext.cs
  Data/Configurations/  one file per entity
  Data/Migrations/   generated
  Services/          AdminSeeder.cs
tests/InchesWigStore.Tests/
  InchesWigStore.Tests.csproj
  Infrastructure/    SqlServerFixture.cs, TestData.cs
  Data/              CatalogueSchemaTests.cs, IdentitySchemaTests.cs,
                     OrderSchemaTests.cs, PaymentStockSchemaTests.cs,
                     MigrationTests.cs
  Services/          AdminSeederTests.cs
  Web/               HealthEndpointTests.cs
docs/architecture.md, style-guide.md, database-design.md, decisions.md
```

---

### Task 1: Solution scaffold, build rules and CI

**Files:**
- Create: `global.json`, `Directory.Build.props`, `.editorconfig`, `InchesWigStore.sln`, `src/InchesWigStore.Web/*` (MVC template), `tests/InchesWigStore.Tests/*`, `.github/workflows/ci.yml`
- Modify: `.gitignore`

**Interfaces:**
- Produces: the two projects above; `Directory.Build.props` setting `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `AnalysisLevel=latest-recommended`; env var contract `TEST_SQL_CONNECTION` (SQL Server connection string whose login may create and drop databases) used by all later integration tests.

- [ ] **Step 0: Create the working branch**

```bash
git switch -c feat/m1-foundation-and-database
```
(If this plan file is uncommitted on `docs/requirements`, commit it there first or carry it across; do not lose it.)

- [ ] **Step 1: Write the failing smoke test** in `tests/InchesWigStore.Tests/SmokeTests.cs`: `Solution_references_web_project()` asserts `typeof(InchesWigStore.Web.Program).Assembly.GetName().Name == "InchesWigStore.Web"`.

- [ ] **Step 2: Run it to verify it fails**

Run: `dotnet test` · Expected: FAIL (no solution or projects yet).

- [ ] **Step 3: Scaffold.** `dotnet new sln -n InchesWigStore`; `dotnet new mvc -n InchesWigStore.Web -o src/InchesWigStore.Web --auth None`; `dotnet new xunit -n InchesWigStore.Tests -o tests/InchesWigStore.Tests`; add both to the solution; test project references the web project. Make `public partial class Program {}` available at the end of `Program.cs`. Add `global.json`, `Directory.Build.props` and `.editorconfig` (C# naming: `_camelCase` private fields, PascalCase everything else, file-scoped namespaces, `var` where type is obvious) as specified in the Interfaces block.

- [ ] **Step 4: Write `.github/workflows/ci.yml`.** Triggers: `pull_request` and push to `main`. One job on `ubuntu-latest` with a `services.mssql` container (`mcr.microsoft.com/mssql/server:2022-latest`, `ACCEPT_EULA=Y`, a throwaway CI-only `MSSQL_SA_PASSWORD`, port 1433, health command). Steps: checkout, `actions/setup-dotnet` using `global.json`, `dotnet restore`, `dotnet format --verify-no-changes`, `dotnet build -c Release --no-restore`, `dotnet test -c Release --no-build` with `TEST_SQL_CONNECTION=Server=localhost,1433;User Id=sa;Password=<same>;TrustServerCertificate=True`.

- [ ] **Step 5: Run tests and format check locally**

Run: `dotnet format --verify-no-changes && dotnet test` · Expected: PASS, 1 test.

- [ ] **Step 6: Commit**

```bash
git add global.json Directory.Build.props .editorconfig .gitignore InchesWigStore.sln src tests .github
git commit -m "build: solution scaffold, analyser rules and CI"
```

(After the first PR is open, the owner turns on branch protection requiring this workflow. This is a GitHub setting, not code; note it in the PR.)

---

### Task 2: Test database fixture, DbContext and the catalogue schema

**Files:**
- Create: `src/InchesWigStore.Web/Models/{ApplicationUser,Category,Product,ProductVariant,ProductImage,TryOnAsset}.cs`, `Data/AppDbContext.cs`, `Data/Configurations/*` for those entities, first migration
- Create: `tests/InchesWigStore.Tests/Infrastructure/{SqlServerFixture,TestData}.cs`, `Data/CatalogueSchemaTests.cs`, `Data/MigrationTests.cs`
- Modify: `src/InchesWigStore.Web/InchesWigStore.Web.csproj` (EF Core SqlServer, Design, Identity.EntityFrameworkCore packages)

**Interfaces:**
- Consumes: `TEST_SQL_CONNECTION` from Task 1; if unset, fixtures use `Server=(localdb)\MSSQLLocalDB;Integrated Security=true`.
- Produces:
  - `class ApplicationUser : IdentityUser<Guid> { string DisplayName; DateTimeOffset CreatedAt; }`
  - `class AppDbContext(DbContextOptions<AppDbContext>) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` with `DbSet`s named as the plural entity names (`Categories`, `Products`, `ProductVariants`, `ProductImages`, `TryOnAssets`, and later ones).
  - `Category { int Id; string Name; string Slug; int SortOrder; bool IsActive }`; `Slug` unique.
  - `Product { int Id; int CategoryId; string Name; string Slug; string Description; bool IsActive; DateTimeOffset CreatedAt }`; `Slug` unique.
  - `ProductVariant { int Id; int ProductId; string Sku; long PriceCents; int StockQuantity; bool IsActive; int? LengthInches; string? Texture, Colour, Density, CapSize, LaceType, Size, Style; byte[] RowVersion }`
  - `ProductImage { int Id; int ProductId; string StoragePath; string AltText; int SortOrder; bool IsMain }`
  - `TryOnAsset { int Id; int ProductId; string Colour; string StoragePath; double OpeningX, OpeningY, OpeningWidth, OpeningHeight }`; unique on `(ProductId, Colour)`.
  - `SqlServerFixture : IAsyncLifetime` — creates a uniquely named database, runs `Database.MigrateAsync()`, exposes `AppDbContext CreateContext()`, drops the database on dispose. Used as `IClassFixture<SqlServerFixture>`.
  - `TestData`: `Category Category(string? slug = null)`, `Product Product(Category c, string? slug = null)`, `ProductVariant Variant(Product p, string? sku = null, long priceCents = 100_00, int stock = 5)`; each generates unique slugs/SKUs when none given.

- [ ] **Step 1: Write the failing tests** in `CatalogueSchemaTests` (each saves through a context from the fixture; expectations use `Assert.ThrowsAsync<DbUpdateException>` unless stated):
  - `Sku_is_unique_ignoring_case` — save variant `"wig-001"`, then another with `"WIG-001"` → throws.
  - `Duplicate_option_combination_on_same_product_is_refused` — two variants of one product both 22 in / "Wavy" / "Black" → throws; the same combination on a different product saves.
  - `Accessory_with_no_options_saves_once_but_second_identical_is_refused` — two variants of one product with every option null → second throws.
  - `Negative_stock_is_refused` — `StockQuantity = -1` → throws.
  - `Negative_price_is_refused` — `PriceCents = -1` → throws.
  - `Zero_stock_and_zero_price_are_allowed` — saves without error.
  - `Concurrent_stock_updates_raise_a_concurrency_error` — load the same variant in two contexts, change `StockQuantity` in both, save the first (ok), save the second → `DbUpdateConcurrencyException`.
  - `Duplicate_category_slug_and_product_slug_are_refused`.
  - `TryOnAsset_is_unique_per_product_and_colour`.
  - `Variant_with_a_product_cannot_outlive_it` — deleting a product with variants and no cascade declared by this plan is refused (relationship is `Restrict`; products are deactivated, not deleted).

  In `MigrationTests`: `Model_has_no_pending_changes` asserts `!context.Database.HasPendingModelChanges()`; `Migrations_apply_to_an_empty_database` is implicitly covered by the fixture.

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test --filter "FullyQualifiedName~CatalogueSchemaTests"` · Expected: FAIL (types do not exist).

- [ ] **Step 3: Implement entities, `AppDbContext` and one `IEntityTypeConfiguration<T>` per entity** with the shapes above. Required specifics: `Sku` `nvarchar(64)` with a unique index (default collation gives case-insensitivity); `HasCheckConstraint("CK_ProductVariant_Stock_NonNegative", "[StockQuantity] >= 0")` and the same for price; a unique index on `(ProductId, LengthInches, Texture, Colour, Density, CapSize, LaceType, Size, Style)` (SQL Server treats nulls as equal in unique indexes); `RowVersion` configured with `IsRowVersion()`; all foreign keys `DeleteBehavior.Restrict`; indexes on `ProductVariant(LengthInches)`, `(Texture)`, `(Colour)`, `(PriceCents)` and on `Product(CategoryId, IsActive)` to support filtering.

- [ ] **Step 4: Implement `SqlServerFixture` and `TestData`** per the Interfaces block. Add EF Core packages (pin the 10.x versions that `dotnet add package` resolves) and `dotnet tool install dotnet-ef` as a local tool (`dotnet new tool-manifest`).

- [ ] **Step 5: Create the migration**

Run: `dotnet ef migrations add Catalogue --project src/InchesWigStore.Web --output-dir Data/Migrations`

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test` · Expected: PASS, all `CatalogueSchemaTests` and `MigrationTests`.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat(data): catalogue schema with SKU, stock and option constraints (FR-VAR-1, FR-VAR-2, FR-VAR-5)"
```

---

### Task 3: Addresses, roles and the seeded administrator

**Files:**
- Create: `Models/Address.cs`, `Data/Configurations/AddressConfiguration.cs`, `Services/AdminSeeder.cs`, migration `Addresses`
- Test: `tests/InchesWigStore.Tests/Data/IdentitySchemaTests.cs`, `Services/AdminSeederTests.cs`

**Interfaces:**
- Consumes: `AppDbContext`, `ApplicationUser`, `SqlServerFixture` from Task 2.
- Produces:
  - `Address { int Id; Guid UserId; string RecipientName, Phone, Line1; string? Line2; string City, Province, PostalCode; bool IsDefault }`; FK to user `Restrict`; index on `UserId`.
  - `static class Roles { const string Administrator = "Administrator"; const string Customer = "Customer"; }`
  - `class AdminSeeder(UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles)` with `Task SeedAsync(string? email, string? password)`: creates both roles if missing; if `email` and `password` are both non-empty and no user with that email exists, creates the user in the `Administrator` role with a confirmed email. Does nothing for the admin part when either value is missing. Never logs the password. Idempotent.

- [ ] **Step 1: Write the failing tests.**
  - `IdentitySchemaTests`: `Address_requires_an_existing_user` (insert with random `UserId` → `DbUpdateException`); `Deleting_a_user_with_addresses_is_refused`.
  - `AdminSeederTests` (uses a real `ServiceProvider` with Identity on the fixture database): `Creates_roles_and_one_administrator`; `Running_twice_creates_one_administrator`; `Missing_password_creates_roles_but_no_user`; `Seeded_user_is_only_in_the_Administrator_role` (not `Customer`); `Weak_password_fails_with_an_error_and_creates_no_user` (password `"short"` → throws `InvalidOperationException` whose message contains no part of the password).

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test --filter "FullyQualifiedName~IdentitySchemaTests|FullyQualifiedName~AdminSeederTests"` · Expected: FAIL.

- [ ] **Step 3: Implement** `Address`, its configuration, `Roles`, and `AdminSeeder`. Identity options (to be reused by `Program.cs` in Task 6, so put them in a static `IdentityDefaults.Configure(IdentityOptions)` in `Services/`): `RequiredLength = 12`, `Lockout.MaxFailedAccessAttempts = 5`, `Lockout.DefaultLockoutTimeSpan = 15 minutes`, `User.RequireUniqueEmail = true` (FR-ACC-2, FR-ACC-3).

- [ ] **Step 4: Add migration** — `dotnet ef migrations add Addresses --project src/InchesWigStore.Web --output-dir Data/Migrations`.

- [ ] **Step 5: Run the full test suite** — Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(identity): addresses, roles and administrator seeder (FR-ACC-8)"
```

---

### Task 4: Cart and order schema (snapshots, statuses, history)

**Files:**
- Create: `Models/{Cart,CartItem,Order,OrderItem,DeliveryAddressSnapshot,OrderStatusHistory,Enums}.cs`, matching `Data/Configurations/*`, migration `CartsAndOrders`
- Test: `tests/InchesWigStore.Tests/Data/OrderSchemaTests.cs`
- Modify: `Data/AppDbContext.cs` (new `DbSet`s), `TestData.cs` (add `Order Order(ApplicationUser u, params ProductVariant[] v)` building a valid order with items and snapshot)

**Interfaces:**
- Consumes: Task 2 and 3 types; `TestData` helpers.
- Produces (`Enums.cs`):
  - `enum OrderStatus { Pending, Confirmed, Processing, Shipped, Delivered, Cancelled }` (stored as `string`)
  - `enum PaymentStatus { Pending, Paid, Failed, Refunded }` (stored as `string`)
  - `enum PaymentMethod { Eft, Online }` (stored as `string`)
  - `Cart { int Id; Guid? UserId; string? VisitorToken; DateTimeOffset UpdatedAt }` — exactly one of `UserId` / `VisitorToken` set (check constraint); `UserId` unique where not null.
  - `CartItem { int Id; int CartId; int VariantId; int Quantity }` — unique `(CartId, VariantId)`; `Quantity > 0`.
  - `DeliveryAddressSnapshot` (owned type, columns prefixed `Ship`): `RecipientName, Phone, Line1, Line2?, City, Province, PostalCode`.
  - `Order { int Id; string Reference; Guid UserId; OrderStatus Status; PaymentStatus PaymentStatus; PaymentMethod PaymentMethod; long SubtotalCents, DeliveryFeeCents, TotalCents; DeliveryAddressSnapshot DeliveryAddress; DateTimeOffset CreatedAt }` — `Reference` unique, `nvarchar(20)`; check `TotalCents = SubtotalCents + DeliveryFeeCents`; all cents `>= 0`; FK to user `Restrict`.
  - `OrderItem { int Id; int OrderId; int VariantId; string ProductName; string Sku; string OptionsText; int Quantity; long UnitPriceCents }` — `Quantity > 0`, `UnitPriceCents >= 0`; FK to variant `Restrict`; FK to order `Cascade`.
  - `OrderStatusHistory { int Id; int OrderId; string Kind /* "Order" | "Payment" */; string FromStatus; string ToStatus; string ChangedBy; DateTimeOffset ChangedAt }`.

- [ ] **Step 1: Write the failing tests** in `OrderSchemaTests`:
  - `Order_keeps_its_snapshot_when_variant_and_product_change` — save an order, then change the variant's `PriceCents` and the product's `Name`, reload the order: item `ProductName`, `Sku`, `UnitPriceCents` are unchanged (FR-ORD-6).
  - `Order_total_must_equal_subtotal_plus_delivery_fee` — mismatch → `DbUpdateException`.
  - `Order_reference_is_unique`.
  - `Order_item_quantity_zero_or_negative_is_refused` and `Cart_item_quantity_zero_is_refused`.
  - `Variant_that_is_on_an_order_cannot_be_deleted` and `Product_that_is_on_an_order_cannot_be_deleted` (the latter via its variant) — both throw (Review Focus 1).
  - `User_with_an_order_cannot_be_deleted` (Review Focus 1).
  - `Order_and_payment_statuses_change_independently_and_are_stored_as_text` — set `Status = Shipped`, `PaymentStatus = Paid`, reload, assert both; assert the raw column value is `"Shipped"`.
  - `Status_history_rows_are_kept_in_order` — add two history rows, query by `OrderId` ordered by `ChangedAt`, assert `FromStatus`/`ToStatus` sequence.
  - `Deleting_an_order_removes_its_items_but_not_the_variant`.
  - `Cart_must_belong_to_exactly_one_of_user_or_visitor` — both null → throws; both set → throws.
  - `Cart_has_one_row_per_variant` — duplicate `(CartId, VariantId)` → throws.

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test --filter "FullyQualifiedName~OrderSchemaTests"` · Expected: FAIL.

- [ ] **Step 3: Implement entities, configurations (string conversions for the enums, owned type for the snapshot, the check constraints and unique indexes listed above) and `TestData.Order`.**

- [ ] **Step 4: Add migration** — `dotnet ef migrations add CartsAndOrders --project src/InchesWigStore.Web --output-dir Data/Migrations`.

- [ ] **Step 5: Run the full suite** — Expected: PASS (including `Model_has_no_pending_changes`).

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(data): cart and order schema with snapshots and status history (FR-ORD-5..7)"
```

---

### Task 5: Payment, notification and stock-movement schema

**Files:**
- Create: `Models/{Payment,PaymentNotification,StockMovement}.cs`, matching configurations, migration `PaymentsAndStockMovements`
- Test: `tests/InchesWigStore.Tests/Data/PaymentStockSchemaTests.cs`
- Modify: `Data/AppDbContext.cs`, `Models/Enums.cs`

**Interfaces:**
- Consumes: `Order`, `ProductVariant`, enums from Task 4.
- Produces:
  - `enum StockMovementReason { Sale, Cancellation, EftExpired, AdminAdjustment, Initial }` (stored as `string`)
  - `Payment { int Id; int OrderId; PaymentMethod Method; PaymentStatus Status; long AmountCents; long? AmountReceivedCents; string Reference; string? ProviderReference; string? VerifiedBy; DateTimeOffset? VerifiedAt; DateTimeOffset CreatedAt }` — `Reference` unique; `ProviderReference` unique where not null; amounts `>= 0`.
  - `PaymentNotification { int Id; string Provider; string NotificationId; int? PaymentId; DateTimeOffset ReceivedAt; string Outcome }` — unique `(Provider, NotificationId)`.
  - `StockMovement { int Id; int VariantId; int OldQuantity; int NewQuantity; StockMovementReason Reason; string ChangedBy; int? OrderId; DateTimeOffset ChangedAt }` — quantities `>= 0`; FK to variant `Restrict`; FK to order `Restrict`, nullable.
  - There is no card-number or card-security-code column anywhere (FR-PAY-12).

- [ ] **Step 1: Write the failing tests** in `PaymentStockSchemaTests`:
  - `Same_provider_notification_id_twice_is_refused` — second insert throws; the same `NotificationId` under a different `Provider` saves (FR-PAY-10).
  - `Payment_reference_is_unique`.
  - `Two_payments_without_provider_reference_are_allowed_but_duplicate_provider_reference_is_refused` (filtered unique index).
  - `Eft_payment_can_record_amount_received_different_from_amount` — `AmountCents = 100_00`, `AmountReceivedCents = 90_00` saves and reloads with both values (FR-PAY-4 groundwork).
  - `Stock_movement_records_old_new_reason_and_actor` — save and reload; assert all fields, `Reason` stored as text `"AdminAdjustment"` (FR-VAR-6).
  - `Stock_movement_with_negative_quantity_is_refused`.
  - `Variant_with_stock_movements_cannot_be_deleted`.
  - `Schema_has_no_column_that_could_hold_card_data` — read `context.Model` and assert no property name contains `Card`, `Pan`, `Cvv` or `Cvc` (case-insensitive) (FR-PAY-12).

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test --filter "FullyQualifiedName~PaymentStockSchemaTests"` · Expected: FAIL.

- [ ] **Step 3: Implement entities and configurations** per the Interfaces block (filtered unique index for `ProviderReference` with `HasFilter("[ProviderReference] IS NOT NULL")`).

- [ ] **Step 4: Add migration** — `dotnet ef migrations add PaymentsAndStockMovements --project src/InchesWigStore.Web --output-dir Data/Migrations`.

- [ ] **Step 5: Run the full suite** — Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(data): payment, notification and stock movement schema (BR-6, BR-13)"
```

---

### Task 6: Web composition root and health check

**Files:**
- Modify: `src/InchesWigStore.Web/Program.cs`, `appsettings.json`
- Test: `tests/InchesWigStore.Tests/Web/HealthEndpointTests.cs`

**Interfaces:**
- Consumes: `AppDbContext`, `IdentityDefaults.Configure`, `AdminSeeder`, `Roles` from earlier tasks; `Microsoft.AspNetCore.Mvc.Testing` package added to the test project.
- Produces: connection string name `DefaultConnection` (value in `appsettings.json` is LocalDB for development only; real values come from user-secrets or environment); `GET /health` returning `200` with body `Healthy` when `AppDbContext` can connect and `503` when it cannot; at startup the app calls `AdminSeeder.SeedAsync` with configuration keys `Admin:Email` and `Admin:Password` (environment variables `Admin__Email`, `Admin__Password`); migrations are **not** applied at startup outside Development (NFR-OPS-1: the deploy step runs `dotnet ef database update`).

- [ ] **Step 1: Write the failing tests** using `WebApplicationFactory<Program>` with the connection string overridden to the fixture database:
  - `Health_returns_200_when_database_is_reachable` — status `200`, body `Healthy`.
  - `Health_returns_503_when_database_is_unreachable` — connection string pointing at a non-existent server with a 1-second timeout → status `503`.
  - `Admin_is_seeded_from_configuration_at_startup` — factory configured with `Admin:Email` and a 12+ character password; afterwards `UserManager` finds that user in `Administrator`.
  - `Home_page_still_renders` — `GET /` returns `200`.

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test --filter "FullyQualifiedName~HealthEndpointTests"` · Expected: FAIL.

- [ ] **Step 3: Implement `Program.cs`:** `AddDbContext<AppDbContext>` (SQL Server, `DefaultConnection`), `AddIdentity`/`AddDefaultTokenProviders` with `IdentityDefaults.Configure`, `AddHealthChecks().AddDbContextCheck<AppDbContext>()` mapped to `/health` with `Healthy`/`Unhealthy` plain-text responses and `503` for unhealthy, `AdminSeeder` registered and run once after build. In Development only, apply pending migrations. Document in a code comment that secrets use `dotnet user-secrets`, never `appsettings.json`.

- [ ] **Step 4: Run the full suite and `dotnet format --verify-no-changes`** — Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(web): composition root, identity options and health check (NFR-OPS-2)"
```

---

### Task 7: Documentation and README status

**Files:**
- Create: `docs/architecture.md`, `docs/style-guide.md`, `docs/database-design.md`, `docs/decisions.md`
- Modify: `README.md` (Project Status section; documentation table links)

**Interfaces:**
- Consumes: the final entity list, constraints and migrations from Tasks 2 to 5.
- Produces: documentation satisfying NFR-QUA-6.

- [ ] **Step 1: `docs/database-design.md`** — an entity-relationship diagram (Mermaid `erDiagram`) of every entity, a table per entity listing columns, keys, indexes and constraints exactly as migrated, and a "rules enforced by the database" table mapping each constraint to its BR/FR id and the test that pins it. Check it against the generated migrations; any column present in one and not the other is a bug.

- [ ] **Step 2: `docs/decisions.md`** — dated entries, each with context, decision, consequence:
  1. Money stored as `bigint` cents, not `decimal`.
  2. Variant options as nullable typed columns with a unique combination index, not a generic attribute table (filters and uniqueness stay simple; adding an option is a migration).
  3. Foreign keys default to `Restrict`; products and variants are deactivated, never deleted; a customer with orders is anonymised in a later milestone, never hard-deleted (NFR-PRV-2).
  4. Integration tests use a real SQL Server (LocalDB locally, service container in CI), not the EF in-memory provider.
  5. Migrations are applied by the deploy step, not at startup (NFR-OPS-1).
  6. Open decisions D-1 to D-8 are unchanged by M1; D-4 (hold time) and D-2 (VAT) will need columns or settings in M3 and M5.

- [ ] **Step 3: `docs/architecture.md`** — the layer diagram from the README, the project layout from the File Structure above, and where each later milestone's code will go. **`docs/style-guide.md`** — the `.editorconfig` rules in words, namespace and folder conventions, test naming (`Behaviour_in_plain_words`), commit message format (`type(scope): summary`), and the "one issue, one PR" rule.

- [ ] **Step 4: Update `README.md`** — status text to "M1 complete: solution, CI and database schema; features not yet built"; link the four new documents in the documentation table.

- [ ] **Step 5: Verify** — run `dotnet test` once more (docs must not break the build) and open each Mermaid diagram in the GitHub preview or a Mermaid renderer to confirm it renders.

- [ ] **Step 6: Commit**

```bash
git add docs README.md
git commit -m "docs: architecture, style guide, database design and decisions for M1"
```

---

## Self-Review

**Spec coverage (M1 rows of the traceability tables):** FR-VAR-1/2/5 → Task 2; FR-VAR-6 (storage side) → Task 5; FR-ORD-5/6/7 (storage side) → Task 4; FR-PAY-10 (storage side) and FR-PAY-12 → Task 5; FR-ACC-8, FR-ACC-2/3 identity options → Task 3; NFR-OPS-2 → Task 6; NFR-QUA-3 to NFR-QUA-6 → Tasks 1 and 7; data model (§7) every entity: User (2), Address (3), Category, Product, ProductVariant, ProductImage, TryOnAsset (2), Cart/CartItem, Order, OrderItem, OrderStatusHistory (4), Payment, PaymentNotification, StockMovement (5). Nothing in §7 is missing.

**Deliberately not in M1:** behaviour (cart rules, checkout, status-transition logic, payment handling), image upload, secret scanning in CI (NFR-SEC-6, M6), security headers, rate limiting. The schema only makes the later rules enforceable.

**Open decisions touching this plan:** none block M1. D-2 (VAT) may add price columns later; D-4 (EFT hold time) is a setting added in M3/M5.
