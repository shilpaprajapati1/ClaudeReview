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
│   ├── Components/
│   │   ├── Pages/            # Routable pages (Home, Counter, Weather)
│   │   └── Layout/           # Shell layout and nav
│   └── wwwroot/              # Static assets
└── .github/workflows/
    ├── claude-pr-review.yml  # Auto-review on PR open/update to main
    └── claude.yml            # On-demand review via @claude mention in comments
```

**Blazor rendering model:** Static SSR by default; `Counter.razor` opts into interactive server rendering (`@rendermode InteractiveServer`). `Weather.razor` uses `[StreamRendering]` for progressive async data display.

**GitHub Actions integration:** `claude-pr-review.yml` triggers on PR events (`opened`, `synchronize`, `reopened`, `ready_for_review`) targeting `main`. It checks out the PR branch with `fetch-depth: 0`, then calls `anthropics/claude-code-action@beta` with a structured `direct_prompt` covering quality, correctness, security, performance, architecture, .NET/Blazor patterns, testing, and documentation. Results post as inline PR comments plus a summary recommendation.

**Required secret:** `ANTHROPIC_API_KEY` must be set in the repo's Actions secrets for the review workflow to function.

## Key Conventions

- **Nullable reference types enabled** — all code must be null-safe.
- **Implicit usings enabled** — avoid redundant `using` statements.
- The Blazor app is intentionally minimal (no external NuGet dependencies beyond the .NET 10 framework) to keep the PR diff clean for review demonstrations.
