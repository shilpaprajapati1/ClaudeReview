# ClaudeReview

A sample .NET Blazor Web Application with automated Claude AI code review on pull requests.

## Overview

This repository contains:
- **BlazorSampleApp/** — A sample .NET 10 Blazor Web App (Server-side rendering + Interactive Server components)
- **.github/workflows/claude-pr-review.yml** — A GitHub Actions workflow that automatically triggers a Claude AI code review whenever a pull request is opened or updated against the `main` branch

## How It Works

```
Developer opens PR  →  GitHub Actions triggers  →  Claude reviews code  →  Review posted as PR comment
     (any branch)           (on PR to main)          (via Anthropic API)
```

When a pull request is submitted (or updated) targeting the `main` branch, the **Claude PR Review** GitHub Actions workflow:

1. Checks out the repository
2. Invokes `anthropics/claude-code-action` with a structured review prompt
3. Claude analyses the diff and posts a detailed review comment directly on the PR, covering:
   - Code quality & best practices
   - Correctness & potential bugs
   - Security vulnerabilities
   - Performance considerations
   - .NET / Blazor-specific patterns
   - Testing & documentation gaps
4. Automatically submits a **Request Changes** review, **blocking the merge** until a maintainer approves

## Setup

### 1. Add your Anthropic API Key

Add your Anthropic API key as a repository secret:

1. Go to **Settings → Secrets and variables → Actions**
2. Click **New repository secret**
3. Name: `ANTHROPIC_API_KEY`
4. Value: your Anthropic API key (get one at https://console.anthropic.com)

### 2. Disable GitHub Copilot Code Review (use Claude only)

To ensure only Claude performs code review (not GitHub Copilot):

1. Go to **Settings → Code review** (requires repository admin access)
2. Under **Copilot code review**, toggle it **off**

> This ensures all automated review feedback comes exclusively from Claude.

### 3. Require the Claude review as a status check (enforce merge blocking)

To enforce that the Claude review must pass before merging:

1. Go to **Settings → Branches**
2. Click **Add branch protection rule** (or edit the existing rule for `main`)
3. Enable **Require a pull request before merging**
4. Enable **Require approvals** (set to 1 or more)
5. Under **Require status checks to pass before merging**, add `Claude Code Review`
6. Save the rule

With this in place, every PR to `main` is blocked until:
- Claude has completed its review and posted its findings
- A maintainer reviews Claude's comments and approves the PR

### 4. Create a working branch and open a PR

```bash
git checkout -b feature/my-feature
# make changes …
git push origin feature/my-feature
# Open a Pull Request targeting `main`
```

The Claude review will be posted automatically as a comment on the PR.

## Blazor Sample App

The `BlazorSampleApp` is a standard .NET 10 Blazor Web App with:

| Page | Route | Description |
|------|-------|-------------|
| Home | `/` | Welcome page |
| Counter | `/counter` | Interactive counter (demonstrates Blazor interactivity) |
| Weather | `/weather` | Weather forecast table (demonstrates data rendering) |

### Running Locally

```bash
cd BlazorSampleApp
dotnet run
```

Open https://localhost:5001 in your browser.

## Requirements

- .NET 10 SDK or later
- An Anthropic API key for the Claude PR review workflow

> **Note:** This PR was created to verify the Claude automated review workflow triggers correctly on new pull requests.
