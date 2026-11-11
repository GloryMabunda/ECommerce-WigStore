# Style guide

These are the conventions for code, tests, the database and the way we work. Most are enforced by tools: `.editorconfig` and the analysers are checked in the build, and CI runs `dotnet format --verify-no-changes` (NFR-QUA-4). A pull request that breaks them fails CI.

## 1. Formatting and code style (`.editorconfig`)

The file at the repository root is the source of truth. In words:

- **All files:** spaces for indentation, UTF-8, a final newline, and no trailing whitespace.
- **C# files:** four-space indent. File-scoped namespaces (`namespace X;`) are required; using the block form is a warning, which the build treats as an error.
- **JSON, YAML, `.csproj`, `.props`, `.cshtml`, HTML, CSS and JavaScript:** two-space indent.
- **`var`:** prefer `var` when the type is obvious from the right-hand side (`var user = new ApplicationUser()`); write the type for built-in types (`int count = 0`) and where the type is not apparent. These are suggestions, not errors.
- **Naming:** private fields are `_camelCase` (underscore prefix). Classes, structs, interfaces, enums, properties, methods, events, namespaces and delegates are `PascalCase`. Both are warnings, so errors in the build.
- **Migrations** (`Data/Migrations/**`) are generated code and are marked so; do not edit them by hand and do not reformat them.
- **Test projects:** the rule against underscores in member names (CA1707) is switched off, so test names can use underscores.

`Directory.Build.props` adds, for every project: nullable reference types on, implicit usings on, warnings as errors, code-style enforcement in the build, and the latest recommended analysers. Do not suppress a warning to make a build pass without a comment saying why.

Before you push, run `dotnet format` and `dotnet build`.

## 2. Namespaces and folders

- The root namespace is `InchesWigStore.Web` for the application and `InchesWigStore.Tests` for the tests.
- A namespace matches its folder: `Models/Order.cs` is in `InchesWigStore.Web.Models`; `Data/Configurations/OrderConfiguration.cs` is in `InchesWigStore.Web.Data.Configurations`; `Services/AdminSeeder.cs` is in `InchesWigStore.Web.Services`.
- One top-level type per file, and the file is named after the type.
- The one exception is the `Program` type, which is in the global namespace so tests can reference it (decision 8 in [`decisions.md`](decisions.md)).
- Entity configuration is not written in `OnModelCreating` or with attributes. Each entity has an `<Entity>Configuration` class in `Data/Configurations/`, which `AppDbContext` finds automatically.
- Business rules go in services, not controllers or views. Controllers stay thin.
- Strings in entities that map to database columns always get an explicit maximum length in the configuration (decision 3).
- Money is `long` cents, and the property name ends in `Cents` (decision 1).

## 3. Database naming

- Tables are plural (`Orders`, `ProductVariants`); EF Core takes them from the `DbSet` names.
- **Check constraints:** `CK_<PluralTable>_<Rule>`, for example `CK_Orders_Total_Equals_Subtotal_Plus_Delivery`, `CK_CartItems_Quantity_Positive`, `CK_Carts_Exactly_One_Owner`. Use underscores between the words of the rule.
- **Indexes:** `IX_<Table>_<Columns>`, for example `IX_Orders_Reference` and `IX_CartItems_CartId_VariantId`. Give the name explicitly with `HasDatabaseName` when a test needs to name it. Filtered or unique-with-`NULL` indexes must say so in a comment (see `ProductVariantConfiguration`).
- **Foreign keys** keep EF Core's generated names, `FK_<Child>_<Parent>_<Column>`, which the tests use.
- **Exception to remember:** the two earliest check constraints use the singular table name, `CK_ProductVariant_Stock_NonNegative` and `CK_ProductVariant_Price_NonNegative`. They were named before the convention was settled and are kept to avoid a migration with no behaviour change. New constraints use the plural.
- Every foreign key is `Restrict` unless a decision says otherwise (decision 4).
- Every schema change comes with a migration, an update to [`database-design.md`](database-design.md), and a test that pins the rule.

## 4. Tests

- **Names** say the behaviour in plain words, with underscores: `Behaviour_in_plain_words`. Examples from the project: `Negative_stock_is_refused`, `Order_total_must_equal_subtotal_plus_delivery_fee`, `Health_returns_503_when_database_is_unreachable`. A test name is a sentence about what should be true, not the method it calls.
- **Layout:** the folder mirrors the thing tested (`Data/`, `Services/`, `Web/`), and the class is named `<Subject>Tests` (`OrderSchemaTests`).
- **Arrange, act, assert,** one behaviour per test.
- **Refusal tests.** When a test proves that the database refuses something, it asserts the SQL Server error number and the name of the constraint or index, using the `SqlAssert` helper, so the test fails if some other rule refuses the save:
  - a check constraint or foreign key violation is error `547`: `await AssertRefusedAsync(context, 547, "CK_OrderItems_Quantity_Positive");`
  - a unique index violation is error `2601` or `2627`: `await SqlAssert.AssertUniqueViolationAsync(context, "IX_Payments_Reference");`
  - To prove a foreign key refuses a delete, use a fresh context, so the database and not EF's change tracking refuses it.
  - Asserting only `DbUpdateException` is not enough. (The earliest catalogue tests do this and are marked for tightening in [`database-design.md`](database-design.md).)
- Also test the boundary that must still work (for example zero stock is allowed, a payment without a provider reference is allowed).
- Integration tests use the real SQL Server through `SqlServerFixture`; see [`architecture.md`](architecture.md). Create test data with `TestData` so names and slugs do not collide.
- Do not use the EF in-memory provider.
- Test data and passwords in tests are fake. Never commit a real credential.

## 5. Commits, issues and pull requests

- **One issue, one pull request.** Each change starts from an issue (NFR-QUA-5). A pull request addresses that issue and nothing else. If you find something unrelated, open another issue.
- **Commit message format:** `type(scope): summary`, in the imperative or descriptive present, lower case after the colon, no full stop, and with the requirement id in brackets when one applies. Types: `feat`, `fix`, `docs`, `test`, `refactor`, `build`, `chore`. Scopes used so far: `web`, `data`, `identity`. Examples:
  - `feat(web): composition root, identity options and health check (NFR-OPS-2)`
  - `feat(data): cart and order schema with snapshots and status history (FR-ORD-5..7)`
  - `docs: architecture, style guide, database design and decisions for M1`
- **AI assistance.** When an AI tool helped write a commit, end the message with a `Co-Authored-By:` trailer naming it. The author still reviews and is responsible for every line.
- **Pull request description:** what changed, why, which requirement ids it serves, and how it was tested.
- **Before opening:** the build passes with no warnings, `dotnet format --verify-no-changes` passes, all tests pass, and any document the change affects is updated (NFR-QUA-6).
- **Branches:** the `main` branch is meant to require CI to pass before merging (NFR-QUA-3); set this in the GitHub branch protection rules. Work on a branch and open a pull request.

## 6. Documentation

- Documents are in `docs/` and written in plain language: short sentences, no unexplained jargon, South African English spelling (colour, authorise, anonymise).
- A technical decision gets an entry in [`decisions.md`](decisions.md): the context, the decision and the consequence.
- Diagrams use Mermaid in a fenced `mermaid` block so they render on GitHub and stay in version control. Do not commit images of diagrams.
- Secrets never appear in code, tests, documents, issues or screenshots. Use `dotnet user-secrets` locally and environment variables elsewhere.
