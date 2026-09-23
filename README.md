# ☕ RollUp — Multi-Tenant, Multi-Branch Cafe Management Platform

A real-time cafe and food-truck operations platform built on **Blazor Server** and
**ASP.NET Core**, covering the full loop from a customer scanning a table QR code to
kitchen staff clearing the order off a live Kanban board — across as many branches as a
tenant operates, with live cross-branch analytics and a dark, brand-tinted UI throughout.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](#license)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](#tech-stack)
[![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)](#tech-stack)
[![Styling](https://img.shields.io/badge/Styling-Tailwind%20CSS-38BDF8?logo=tailwindcss)](#design-system)
[![Realtime](https://img.shields.io/badge/Realtime-SignalR-2E7D32)](#real-time-engine)

---

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Features](#features)
- [Design System](#design-system)
- [Onboarding Flow](#onboarding-flow)
- [Navigation & Routes](#navigation--routes)
- [Multi-Branch Management](#multi-branch-management)
- [Real-Time Engine](#real-time-engine)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Authentication & Route Guards](#authentication--route-guards)
- [Getting Started (from source)](#getting-started-from-source)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

RollUp streamlines the entire cafe workflow: a customer browses a dynamic, tag-searchable
menu (via a table QR code or a shared link), places an order with sizes and add-ons, and
watches it move through **Pending → Cooking → Ready** in real time. Kitchen staff work off
a live Kanban queue with audio alerts for new tickets; admins manage menu items, branches,
team members, and branding from a dedicated dashboard that updates every connected screen
the instant a setting changes — no reload, no stale tab.

A tenant can run **multiple branches** (cafés, food trucks, kiosks, bakeries, bistros,
restaurants) under one account, switch which branch is "active" from anywhere in the app,
and see queue, insights, and QR codes scope themselves to that branch automatically.

The app runs self-contained against SQLite out of the box (seeded with a demo tenant,
outlet, categories, and admin account), and is built to run against PostgreSQL in
production.

## Quick Start

```bash
dotnet run --project RollUp.csproj
```

The app comes up at **http://localhost:5000**, backed by a seeded SQLite database
(`rollup.db`).

**Demo login:**
| | |
|---|---|
| Email | `admin@rollup.com` |
| Password | `Admin123!` |

A one-click demo-login button is also available directly on `/login`.

## Features

### Customer Experience
- Categorized, tag-searchable menu with popular-item highlights
- Item variants (sizes) and customizable add-ons
- Cart that automatically merges identical items (matching variant + add-ons)
- Live order tracking (Pending → Cooking → Ready), synced over SignalR
- Three switchable menu layouts (see [Design System](#design-system))
- Table-specific ordering via `/order/{TableNumber}`

### Kitchen Operations
- Three-column Kanban queue (New Orders / Preparing / Ready) on desktop, a segmented
  tab switcher with count badges on mobile
- Audio alert on every incoming order
- Branch-scoped: the queue only shows orders for the currently active branch
- Dedicated read-only display route (`/display`) for a kitchen monitor/TV

### Admin & Operations
- **Menu** — item catalog with category filters, search, sold-out toggles, and a
  **Design Studio** for live-previewing theme, layout, typography, and accent color
- **Insights** — revenue/order-count trend chart, peak rush-hour bar chart, top
  products, category share breakdown, with Today / This Week / This Month / All Time
  and per-branch filtering
- **Share** — Menu QR, per-table QR standees, and a printable paper menu, all scoped to
  the active branch
- **Manage** — branches & profile, categories, team members & roles, accepted payment
  methods, currency settings

## Design System

The app defaults to a **dark theme app-wide**, driven entirely by CSS custom properties
so a tenant's brand color propagates live to every surface without a rebuild or reload.

**Base palette (dark, default):**
| Token | Value | Use |
|---|---|---|
| `--color-bg` | `#09090b` | App background |
| `--color-surface` | `#18181b` | Cards, panels, modals |
| `--color-ink` | `#fafaf9` | Primary text |
| `--color-ink-muted` | `#a1a1aa` | Secondary text |
| `--color-line` | `#27272a` | Borders, dividers |
| `--color-accent` / `--color-accent-ink` | tenant-configurable | Buttons, active states, focus rings |

**Typography:** `Bricolage Grotesque` (display/headings), `DM Sans` (body),
`Fraunces` (editorial/classic style), switchable per tenant via `HeadingFont`.

**Theme presets** (set accent, typography, and palette together): `Bistro Classic`,
`Modern Minimal`, `Rustic Warmth`, `Clean Slate`, `Artisan Luxury` — internally keyed as
`bistro`, `modern`, `rustic`, `minimalist`, `elegant`.

**Menu layouts**, switchable live in Design Studio: `card` (multi-column photo cards),
`list` (editorial horizontal rows), `compact` (fast-order thin rows).

**Live sync:** `IBrandingService.OnBrandingChanged` broadcasts across circuits, so a
branding change made in Design Studio or Setup applies instantly to the admin dashboard
*and* to any open customer menu tab, with no page reload on either side.

**Note on public vs. authenticated shell:** `EmptyLayout` (used for `/welcome`, `/login`,
`/register`, `/setup`) always renders with a black background and the electric-orange
brand accent (`#ff5a1f`), independent of any tenant's saved color. `MainLayout` (the
authenticated workspace) uses the tenant's actual saved `CustomAccentColor`, defaulting to
Emerald Mint (`#10b981`) if none is set — `#ff5a1f` remains available as a selectable
"Electric Orange" swatch, it's just no longer the workspace default.

## Onboarding Flow

New tenants go through a guided path rather than landing straight in the dashboard:

```
/welcome  →  /register  →  /setup (7 steps)  →  /queue
```

- **`/welcome`** — marketing splash, dark background, ambient glow, "Register Café" and
  "Sign In" calls to action.
- **`/register`** — single-page form creating the tenant and owner account; issues a JWT
  and redirects straight into setup.
- **`/setup`** — a 7-step wizard (`WizardStep.razor`), state held in one component, no
  reloads between steps, a Skip option on every step:
  1. **Basics** — business name, outlet type (`Cafe`, `FoodTruck`, `Kiosk`, `Restaurant`)
  2. **Branding** — pick a template preset, fine-tune the accent color, see a live phone
     preview update as you go
  3. **Outlet** — location name, street address, city, phone
  4. **Categories** — preset chips plus free-text custom categories
  5. **Menu items** — add a handful of starter items with price and category
  6. **Payments** — toggle Cash / Card / Mobile wallet
  7. **Review & launch** — a summary card per step with edit-back links, then a
     celebratory completion screen linking into `/queue` or `/menu`
- **`/login`** — redirects straight to `/queue` on success; includes the one-click demo
  login.
- **`/`** — a smart router: authenticated visitors go to `/queue`, anonymous visitors go
  to `/welcome`.

## Navigation & Routes

Every authenticated page runs inside `MainLayout` — a sticky left sidebar on screens
≥900px, a fixed bottom tab bar with count badges on mobile.

| Destination | Primary route | Aliases | Notes |
|---|---|---|---|
| Queue | `/queue` | `/admin/queue`, `/kitchen` | Staff-facing live Kanban |
| Kitchen display | `/display` | `/queue/display` | Read-only board for a kitchen monitor |
| Menu (admin) | `/menu` | — | Items Catalog / Design Studio segmented view |
| Menu (customer) | `/menu/browse`, `/menu/browse/{TableNumber}` | `/order`, `/order/{TableNumber}` | Customer-facing ordering |
| Share | `/share` | — | Menu QR, table QRs, printable menu |
| Insights | `/insights` | `/insight` | Analytics dashboard |
| Manage | `/manage` | — | Branches, categories, team, payments, currency |

`/queue` and `/menu` previously collided with `QueueDisplay.razor` and the customer
`CustomerMenu.razor` component respectively, since both declared the same route. That's
resolved: staff-facing routes keep the short paths, customer/display routes moved to the
paths shown above.

## Multi-Branch Management

A tenant can operate several branches from one account.

- **Add a branch** from `/manage` → Branches & Locations → "+ Add Branch": name, outlet
  type (Café / Bakery / Kiosk / Bistro / Restaurant / Food Truck), address, phone.
- **Branch cards** show type, address, phone, and an "Active Branch" badge with a live
  pulse indicator on whichever branch is currently selected.
- **Branch info panel** (click any card): live queue counts by status, today's order
  count, today's revenue, and quick links into that branch's kitchen queue or customer
  menu.
- **Global switcher**: a branch picker lives under the logo in the desktop sidebar and in
  the mobile header, so switching branches doesn't require a trip to Settings.
- **Switching is instant and app-wide**: Queue re-filters its Kanban board, Insights
  re-scopes its charts and filters, Manage updates its "active" badge, and Share
  regenerates QR codes/standees with the new `?outletId=`, all without a page reload.
- **Tenant isolation is enforced at the service layer** — `OutletService` scopes every
  query to `TenantId`, so one tenant's branches never leak into another's dropdown, queue,
  or analytics.

## Real-Time Engine

SignalR keeps order status and branch context in sync across every open screen:

- Order status changes broadcast to the customer's tracker and the kitchen board
  simultaneously — no polling.
- Kitchen alerts play through `IJSRuntime` calling `wwwroot/js/audio.js`, since Blazor
  Server can't trigger browser audio directly; playback is gated on the browser's
  autoplay policy, enabled after the user's first interaction with the page.
- `IBrandingService.OnBrandingChanged` and `OutletService.OnActiveOutletChanged` extend
  the same "everything stays in sync" model to branding and active-branch state, not just
  order status.

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend framework | Blazor Server (ASP.NET Core 8) |
| Real-time | SignalR (WebSockets) |
| Styling | Tailwind CSS (JIT), semantic CSS-custom-property token system |
| Database | PostgreSQL in production; SQLite fallback/local dev, auto-detected at startup |
| ORM | Entity Framework Core 8 |
| Auth | JWT-based, `[Authorize]` route guards, `AuthorizeRouteView` + `RedirectToLogin` |
| Architecture | Clean Architecture + Repository pattern, multi-tenant with per-tenant data isolation |
| Image storage | Base64-encoded strings, stored directly in the database |

## Architecture

```
┌─────────────────────────────────┐
│   Presentation (Razor Pages)    │   Features/, Shared/, Pages/
├─────────────────────────────────┤
│   Application (DTOs, Services)  │   Application/
├─────────────────────────────────┤
│   Core (Entities, Interfaces)   │   Core/
├─────────────────────────────────┤
│  Infrastructure (DB, Repos)     │   Infrastructure/
└─────────────────────────────────┘
```

Key patterns: repository pattern for data access, dependency injection throughout,
service-layer business logic, SignalR hubs per real-time concern, soft deletes
(`IsDeleted`) instead of hard deletes, eager loading (`.Include()`) to avoid N+1 queries,
and `AsNoTracking()` on read-only queries.

## Project Structure

```
RollUp/
├── API/                          # REST endpoints & SignalR hubs (OrderHub, QueueHub)
├── Application/                  # DTOs & application services
├── Core/
│   ├── Entities/                 # MenuItem, Category, Order, Outlet, Tenant, etc.
│   ├── Services/                 # OutletService, MenuService, OrderNotificationService…
│   └── Interfaces/
├── Infrastructure/
│   ├── Authentication/           # JWT token provider
│   └── Persistence/               # AppDbContext, migrations, tenant-scoping logic
├── Pages/                        # Welcome, Login, Register, Setup, _Layout.cshtml
├── Features/
│   ├── Admin/                    # Menu, Manage, Insights, Share (+ Components/)
│   ├── CustomerMenu/              # Customer-facing ordering UI
│   └── Queue/                    # Staff Kanban + Components/ (OrderKanbanCard, QueueColumn)
├── Shared/
│   ├── Layouts/                  # MainLayout (authenticated), EmptyLayout (public)
│   └── UI/                       # RollUpButton, RollUpModal, WizardStep, RollUpToggleSwitch…
└── wwwroot/
    ├── css/app.css                # Compiled Tailwind output
    └── js/audio.js                # Kitchen alert sound
```

## Authentication & Route Guards

- Every system page (`Queue`, `Menu`, `Manage`, `Insights`, `Share`, `PrintableMenu`,
  `Setup`) carries `@attribute [Authorize]`; `App.razor` wraps routing in
  `<AuthorizeRouteView>` with `<RedirectToLogin />` so an unauthorized route attempt
  redirects to `/login` automatically.
- **Back-button hardening**: logging in navigates to `/queue` with history replaced
  (`replace: true`), so pressing Back afterward doesn't return to `/login`. Logging out
  does the same in reverse into `/login`, and `MainLayout` subscribes to
  `NavigationManager.LocationChanged` to intercept any client-side history navigation
  while signed out.
- `/login`, `/register`, and `/welcome` each guard against an already-authenticated user
  landing on them — they redirect straight to `/queue` instead.
- Email validation on registration is strict (`^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$` plus a
  `MailAddress.TryCreate` check) to reject malformed addresses before an account is created.

## Getting Started (from source)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Node.js (Tailwind build pipeline)
- PostgreSQL for production use (optional locally — SQLite is used automatically if
  PostgreSQL isn't reachable at startup)

### Install & run

```bash
git clone https://github.com/shahsaab/RollUp.git
cd RollUp/RollUp

npm install
dotnet restore

# Build the Tailwind CSS output
npm run build:css

dotnet ef database update
dotnet run --project RollUp.csproj
```

Then open **http://localhost:5000** and log in with the demo credentials above, or
register a new tenant through `/welcome`.

### Configuration

Edit `RollUp/appsettings.json` for your database connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=rollup;User Id=postgres;Password=yourpassword;"
  }
}
```

## Troubleshooting

**A branch or data from another account is showing up**
Tenant scoping depends on `ITenantContext.CurrentTenantId` being populated *before* any
outlet/branch query runs. If you see cross-tenant data, confirm authentication state
resolves before `MainLayout` loads branches — this was a real bug (see the "Tenant Branch
Isolation" fix), now guarded against in `OutletService`, but worth knowing if you're
extending that code path.

**Accent color reverts unexpectedly**
Check that `wwwroot/css/app.css` was rebuilt (`npm run build:css`) after any token change,
and that the tenant's `CustomAccentColor` in the database wasn't overwritten by a bulk
update or migration — `MainLayout` and `CustomerMenu` fall back to Emerald Mint
(`#10b981`) if the saved value is missing, not to orange.

**Audio not playing on new orders**
Confirm the browser tab isn't muted and that the user has interacted with the page at
least once — autoplay restrictions block `audio.js` until then.

**SignalR not updating live**
Confirm WebSockets are enabled on the host and that hub endpoints are mapped correctly in
`Program.cs`; check firewall rules if deploying behind a reverse proxy.

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit with a clear, descriptive message
4. Push and open a Pull Request

Please ensure: code follows standard C# conventions, any schema change ships with a
migration, and new tenant-scoped data goes through the existing tenant-isolation pattern
in `OutletService`/`AppDbContext` rather than a new ad hoc filter.

## License

Licensed under the MIT License — see the `LICENSE` file for details.
