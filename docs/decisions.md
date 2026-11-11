# Technical decisions

Each entry records the context, the decision and its consequences, so that a later change is made knowingly. Entries are numbered and dated. A decision is not edited when it is replaced; a new entry says what it replaces.

Decisions that need the business owner's answer are the open decisions D-1 to D-8 in [`specification.md`](specification.md#13-open-decisions). They are not repeated here, except in entry 15.

## 1. Money is stored as `bigint` cents (2026-11-11)

**Context.** BR-1 requires whole cents, never floating point. `decimal` would also be exact, but it carries a scale and precision that every query, mapping and test must agree on. The README says "appropriate decimal precision"; the specification (section 7) allows `decimal(18,2)` or whole cents, and BR-1 says cents.

**Decision.** Every money column is a `bigint` named `...Cents` (`PriceCents`, `SubtotalCents`, `DeliveryFeeCents`, `TotalCents`, `UnitPriceCents`, `AmountCents`, `AmountReceivedCents`) and the C# property is `long`. The README sentence is superseded by BR-1 and this entry.

**Consequence.** Arithmetic and the `CK_Orders_Total_Equals_Subtotal_Plus_Delivery` check are exact integer sums. Rands appear only at the edge: formatting for display and parsing input. Percentage calculations (VAT, if D-2 requires it) need an explicit rounding rule and a place to store the rate.

## 2. Variant options are nullable typed columns with one combination index (2026-11-11)

**Context.** A wig has length, texture, colour, density, cap size and lace type; an accessory has size, colour or style; a product may have none (BR-2, FR-VAR-3, FR-VAR-4). A generic attribute table (name and value rows) is flexible but makes filtering, uniqueness and reading a variant harder. We need to refuse two variants of one product with the same options (FR-VAR-1).

**Decision.** `ProductVariants` has one nullable column per option: `LengthInches` (int) and `Texture`, `Colour`, `Density`, `CapSize`, `LaceType`, `Size`, `Style` (`nvarchar(50)`). A single unique index covers `ProductId` plus all eight option columns (nine columns). Adding a new option is a migration.

SQL Server treats `NULL` values as equal in a unique index, which is what we want: two variants of one product that both have no options are duplicates. The EF Core SQL Server provider, however, adds a filter `WHERE [col] IS NOT NULL` automatically to a unique index over nullable columns. With that filter, any variant that has a `NULL` option would be left out of the index and could be duplicated without any error. The configuration therefore calls `HasFilter(null)` to remove the default filter, and the generated migration was checked to confirm the index has no filter.

**Consequence.** Filters and uniqueness stay simple and fast, and each option has its own index where needed. The cost is a migration for each new option, and a wide index. The database does not know which options suit which category (BR-2); that is an application rule.

## 3. Option columns have explicit maximum lengths (2026-11-11)

**Context.** SQL Server cannot index `nvarchar(max)`, and a nonclustered index key may not exceed 1,700 bytes. A string property without a length is mapped to `nvarchar(max)` by EF Core.

**Decision.** Every option column is `nvarchar(50)`. The seven text options use at most 700 bytes (7 x 50 characters x 2 bytes), and `ProductId` and `LengthInches` add 8 bytes, well inside the limit. The same habit applies to every string column in the model: an explicit length, chosen to fit the data (for example `Sku` is 64, `Slug` is 100).

**Consequence.** A longer option value is refused by the database. A new option column must be given a length and the key size re-checked.

## 4. Foreign keys are `Restrict`; things are deactivated, not deleted (2026-11-11)

**Context.** Past orders must stay intact (FR-CAT-5, BR-4), stock history must be kept (BR-6), and a person's data may have to be corrected or deleted on request while order records are kept for tax (NFR-PRV-2).

**Decision.** Every foreign key is `Restrict`, so the database refuses to delete a parent that has children. The one exception is `OrderItems` to `Orders`, which is `Cascade`: an order and its lines are one record. Products and variants are deactivated (`IsActive`), never deleted. A customer who has orders is not deleted; in a later milestone the account is anonymised (name, email, addresses and phone removed or replaced) and the order rows stay. `CartItems` to `Carts` and `OrderStatusHistories` to `Orders` are also `Restrict`, not `Cascade`: a deletion that would silently remove child rows should fail until the code removes them deliberately.

**Consequence.** An attempt to delete a variant, product, user, order or payment that is still referenced fails with SQL error 547 and the tests prove it. The cart merge in M3 (FR-CRT-4) must delete the visitor cart's items explicitly before deleting the visitor cart. The M5 or later "delete my data" feature must be written as anonymisation, not `DELETE`.

## 5. Integration tests use a real SQL Server (2026-11-11)

**Context.** NFR-QUA-2 requires integration tests on a real SQL Server. The EF Core in-memory provider does not enforce check constraints, unique filtered indexes, foreign keys, `rowversion` or collation, which are the rules this milestone exists to prove.

**Decision.** Tests create a uniquely named database through `SqlServerFixture`, apply every migration, and drop it afterwards. Locally this is SQL Server LocalDB (`(localdb)\MSSQLLocalDB`). In CI it is a `mssql/server:2022-latest` service container. The connection is taken from the `TEST_SQL_CONNECTION` environment variable when set and defaults to LocalDB. The CI password is a throwaway for a container that exists only for the job.

**Consequence.** Tests need SQL Server (LocalDB on a Windows machine, or Docker). They are slower than in-memory tests, which is accepted. Running the migrations against a fresh database in every fixture also proves the migrations apply from scratch.

## 6. Migrations are applied by the deploy step; the app auto-migrates only in Development (2026-11-11)

**Context.** NFR-OPS-1 says migrations run before a new version starts, and that a failed migration stops the deploy and leaves the running version untouched.

**Decision.** `Program.cs` calls `Database.MigrateAsync()` only when the environment is Development. In every other environment the deploy step runs `dotnet ef database update` (or an equivalent migration bundle) before the new version is started.

**Consequence.** The deploy must migrate **before** it starts the app. Outside Development the app does not migrate, but at startup it does seed the roles and the first administrator when `Admin:Email` and `Admin:Password` are configured. Against an unmigrated database that seed fails (the Identity tables do not exist) and the app does not start. This is intended: it fails visibly instead of running against the wrong schema. The deployment guide (M6) must state the order. If the admin credentials are not configured, no seeding happens and the app starts regardless, so the order matters most for the first deploy.

## 7. EF Core is pinned at 10.0.15 (2026-11-11)

**Context.** The project targets .NET 10 (LTS). EF Core 11 packages require `net11.0`.

**Decision.** All EF Core packages, and the `dotnet-ef` tool in `dotnet-tools.json`, are pinned to 10.0.15 so they cannot drift apart. `global.json` pins the SDK to 10.0.100 with `rollForward: latestFeature`.

**Consequence.** Upgrading to EF Core 11 is a deliberate change made together with moving to .NET 11. Package and tool versions are updated together.

## 8. The `Program` type is in the global namespace (2026-11-11)

**Context.** The web tests use `WebApplicationFactory<Program>`, which needs a public `Program` type. The top-level-statement program generates one in the global namespace.

**Decision.** `Program.cs` ends with `public partial class Program { }` and has no namespace.

**Consequence.** Tests reference `Program` directly. This is the one type outside the `InchesWigStore.Web` namespace, and the style guide says why.

## 9. Statuses are stored as text without a database `CHECK` (2026-11-11)

**Context.** Order, payment and stock-movement reason enums could be stored as numbers or as text, with or without a constraint on the allowed values.

**Decision.** They are stored as `nvarchar(20)` text through EF Core value conversion, with no `CHECK` constraint. The transition rules (BR-11) live in application code and unit tests in M3 to M5.

**Consequence.** Stored data is readable in a query, and reordering an enum cannot change the meaning of stored rows. The database will accept any text up to 20 characters in a status column, so only code that goes through the application can be trusted to write valid statuses. If that is judged too weak, a `CHECK` constraint is a small later migration.

## 10. Variants use a `rowversion` concurrency token (2026-11-11)

**Context.** BR-5 says the last item goes to one buyer. A check constraint stops stock going below zero, but two concurrent updates that each stay above zero could still overwrite one another.

**Decision.** `ProductVariants.RowVersion` is a SQL Server `rowversion`, used by EF Core as a concurrency token, alongside `CK_ProductVariant_Stock_NonNegative`.

**Consequence.** A stale update raises `DbUpdateConcurrencyException`. The checkout code in M3 must either retry or use an atomic conditional update inside its transaction; the test `Concurrent_stock_updates_raise_a_concurrency_error` proves the token works.

## 11. The first administrator is seeded from configuration, and an existing account is promoted only with its password (2026-11-11)

**Context.** FR-ACC-8 requires that the first administrator is created from configuration, not through public registration. `AdminSeeder` runs at startup when `Admin:Email` and `Admin:Password` are both set. It creates the `Administrator` and `Customer` roles, and creates the user if it does not exist. An earlier version promoted any existing user with the configured email without proving control of the account, which would let anyone who registered that email before the first start become an administrator once public registration exists (M3).

**Decision.** If a user with that email already exists, the seeder adds the `Administrator` role only when the user is already in the role (nothing to do) or the configured password matches the account's password (the role is repaired). This keeps a half-seeded database recoverable (`AdminSeederTests.Existing_user_without_the_Administrator_role_is_repaired`). If the account is not an administrator and the password does not match, the seeder logs a warning, promotes nobody and returns (`AdminSeederTests.Existing_user_with_a_different_password_is_not_promoted`). The warning contains neither the email nor the password. Weak passwords for a new administrator make the seeder throw with the Identity error codes.

**Why log and skip rather than throw.** A mismatch is not a startup failure of the site: throwing would let anyone who registers the configured email take the whole application down at its next start. Skipping keeps the site up, the warning tells the operator what happened, and no privilege is granted.

**Consequence.** The configured password is now what proves the operator controls the account, so a stolen or pre-registered account cannot be promoted. Reserving the configured email at registration remains an option for M3 but is no longer needed to close this weakness.

## 12. Identity options follow the requirements (2026-11-11)

**Context.** FR-ACC-2 and FR-ACC-3 set a 12-character minimum password and lock-out after 5 failed sign-ins for 15 minutes.

**Decision.** `IdentityDefaults.Configure` sets `Password.RequiredLength = 12`, `Lockout.MaxFailedAccessAttempts = 5`, `Lockout.DefaultLockoutTimeSpan = 15 minutes` and `User.RequireUniqueEmail = true`. A test pins the values. Users have `Guid` keys.

**Consequence.** A change to these numbers must change the requirement and the test together.

## 13. Health check reports only healthy or unhealthy, with no detail (2026-11-11)

**Context.** NFR-OPS-2 requires a health check that reports whether the database is reachable.

**Decision.** `/health` runs an EF Core `DbContext` check and returns the plain text `Healthy` (200) or `Unhealthy` (503). The body carries no exception or connection detail.

**Consequence.** The endpoint is safe to expose to a host's monitoring. Details are for the logs.

## 14. Documentation lives in `docs/` and is part of the change (2026-11-11)

**Context.** NFR-QUA-6 requires the documents to be kept up to date.

**Decision.** `architecture.md`, `style-guide.md`, `database-design.md` and this file are maintained with the code. A pull request that changes the schema updates `database-design.md`; one that makes a technical decision adds an entry here.

**Consequence.** Review checks the documents as well as the code.

## 15. Open decisions D-1 to D-8 are unchanged by M1 (2026-11-11)

**Context.** The specification lists eight open business decisions (D-1 to D-8). M1 builds the schema and the foundation.

**Decision.** None of D-1 to D-8 was decided in M1. Defaults in the specification stand.

**Consequence.** Two of them will need schema or settings later:

- **D-2 (VAT).** If prices include VAT, orders and order items will need a place to record the VAT amount or rate. That is a migration in M3.
- **D-4 (EFT hold time).** The hold time is a setting. There is no settings table or setting yet; it is added in M3 or M5 with the stock reservation and release code (BR-7). `StockMovementReason.EftExpired` already exists for the release.

D-1 (gateway) affects M4 only. `PaymentNotifications.Provider` is a free text column so a gateway can be chosen later. D-8 (no guest checkout) is already reflected in `Orders.UserId` being required.

## Discrepancies found while documenting M1 (2026-11-11)

Recorded rather than silently resolved. None blocks M1.

- The README says money uses "appropriate decimal precision"; BR-1 and this milestone use whole cents (entry 1).
- Specification section 7 names the entity `OrderStatusHistory`; the table is `OrderStatusHistories` (EF Core pluralisation).
- The README's list of core entities omits carts, cart items, try-on assets and payment notifications; the specification section 7 list is complete and matches the schema.
- Specification BR-2 says the options a variant has depend on its category. The schema does not enforce this.
- Specification BR-11 (status transitions) and BR-7 (stock reservation) are not database-enforced; they are application rules for later milestones.
- Earlier catalogue check constraints use a singular table name (`CK_ProductVariant_...`); later ones use the plural. Renaming would need a migration for no behaviour gain, so the inconsistency is documented in the style guide instead.
- The catalogue refusal tests assert only that a `DbUpdateException` is thrown, not which constraint refused the save. Tightening them is a follow-up.
