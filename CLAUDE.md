# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Purpose

ClaudeReview demonstrates automated Claude AI code review on GitHub pull requests. It pairs a sample .NET 10 Blazor Web App with GitHub Actions workflows that invoke `anthropics/claude-code-action` to post structured review comments on every PR targeting `main`.

## Build & Run

```bash
# Build the solution
dotnet build

# Run the Blazor app (from repo root or BlazorSampleApp/)
cd BlazorSampleApp
dotnet run
```

- HTTP: `http://localhost:5281`
- HTTPS: `https://localhost:7239`

No separate test project exists; the app itself is the demonstration artifact.

## Architecture

```
ClaudeReview/
├── BlazorSampleApp/          # .NET 10 Blazor Web App
│   ├── Program.cs            # ASP.NET Core host setup
│   ├── Models/               # Domain models (Product, Order)
│   ├── Services/             # Business logic (UserService, OrderService)
│   ├── Components/
│   │   ├── Pages/            # Routable pages (Home, Counter, Weather, Products, Orders)
│   │   └── Layout/           # Shell layout and nav
│   └── wwwroot/              # Static assets
└── .github/workflows/
    └── claude.yml            # Auto-review on PR open/update targeting main
```

**Blazor rendering model:** Static SSR by default; `Counter.razor` opts into interactive server rendering (`@rendermode InteractiveServer`). `Weather.razor` uses `[StreamRendering]` for progressive async data display.

**GitHub Actions integration:** `claude.yml` triggers on PR events (`opened`, `synchronize`, `reopened`, `ready_for_review`) targeting `main`. It checks out the full history with `fetch-depth: 0`, then calls `anthropics/claude-code-action@v1` with a structured prompt covering all coding standards below. Results post as a single structured PR comment with severity-tagged findings and fix suggestions.

**Required secret:** `CLAUDE_CODE_OAUTH_TOKEN` must be set in the repo's Actions secrets for the review workflow to function.

## Coding Standards

These standards are enforced by the automated Claude review on every PR to `main`.

### C# Language & Style
- All class members use **properties** (`{ get; set; }`), not public fields
- Naming: PascalCase for types/methods/properties; camelCase for locals/params; `_camelCase` for private fields
- No magic numbers or magic strings — use named constants or enums
- No string concatenation in loops — use `StringBuilder` or string interpolation
- Use `decimal` for currency/financial values, not `double` or `float`
- Use `DateTime.UtcNow` instead of `DateTime.Now` for stored timestamps
- Prefer LINQ over manual `for`/`foreach` loops when it improves readability
- Remove unused fields, variables, and dead code before submitting
- No `TODO` comments in submitted code

### Null Safety & Error Handling
- Nullable reference types must be respected — no `!` suppression without a comment explaining why
- Validate all parameters at public API boundaries (guard clauses)
- Never silently swallow exceptions — log or rethrow
- Use `TryGetValue` for dictionary lookups, not direct indexer access
- Methods that can fail must signal failure explicitly (typed exception or `bool`/`Result<T>` return)

### Async / Threading
- Never use `.Result` or `.Wait()` on a `Task` — always `await`
- Never use `Task.Run(() => ...).Result` inside Blazor lifecycle methods
- Async method names must end with `Async` and return `Task` or `Task<T>`
- Pass `CancellationToken` through where the operation supports cancellation
- `static List<>` and `static Dictionary<>` shared across requests are not thread-safe — use concurrent collections or proper locking

### Security
- No hardcoded credentials, API keys, or secrets in source code
- No plaintext password comparison — use ASP.NET Core Identity or a proper hashing library (e.g., BCrypt)
- All external HTTP calls must use HTTPS
- Never expose internal collections directly to callers — return a copy or `IReadOnlyList<T>`
- Validate user-supplied route parameters (`[Parameter]`) before use
- Do not duplicate authorization logic across components — centralize in a service or policy

### Dependency Injection & HttpClient
- Never instantiate `HttpClient` with `new` — use `IHttpClientFactory`
- Register services with the correct DI lifetime; Singleton services must be thread-safe
- Depend on injected abstractions, not concrete `new`-ed instances inside constructors

### Blazor Patterns
- All data loading must happen in `OnInitializedAsync`, not in `OnInitialized` or inline markup blocks
- No service calls inside the render loop (`@foreach` / `@{ }` markup blocks)
- Always show a loading indicator and an error state during async operations
- Destructive actions (delete, cancel) must include a confirmation step
- Call `StateHasChanged()` only when necessary; add a comment explaining why
- Use relative paths and `NavigationManager` for navigation — no hardcoded absolute URLs

### Performance
- Avoid N+1 patterns — batch data fetching
- Use `<Virtualize>` or pagination for large lists
- Use `StringBuilder` for string building in loops
- Use `System.Text.Json` for JSON serialization — no manual string construction

### Logging & Observability
- Replace `Console.WriteLine` with `ILogger<T>` at the appropriate log level
- Use structured logging — do not use string interpolation inside `LogXxx` calls

### Model Design
- Models use properties, not public fields
- Currency fields are typed `decimal`
- No unused or leftover fields in models
- Consistent naming within each class
- Apply data annotation attributes where input validation is required

## Key Conventions

- **Nullable reference types enabled** — all code must be null-safe.
- **Implicit usings enabled** — avoid redundant `using` statements.
- The Blazor app is intentionally minimal (no external NuGet dependencies beyond .NET 10) to keep PR diffs clean for review demonstrations.
- The existing services (`UserService`, `OrderService`) contain **intentional violations** of the standards above and serve as demonstration targets for the automated review.
