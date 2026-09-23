# RollUp - Cafe Management System

RollUp is a modern, multi-tenant, multi-branch cafe management application designed for
seamless customer ordering and efficient kitchen operations, with a guided onboarding
flow and a live, brand-tinted UI that updates instantly across every connected screen.

## Technical Stack

- **Framework**: Blazor Server (ASP.NET Core 8)
- **Styling**: Tailwind CSS with a semantic, CSS-custom-property-driven design token
  system (dark theme by default; tenant accent color, typography, and layout are all
  runtime-configurable, not hardcoded)
- **Database**: Entity Framework Core with **PostgreSQL** (with automatic SQLite fallback
  for local development and reviewer convenience)
- **Real-time**: SignalR for instant order updates, kitchen notifications, live branding
  sync, and live branch-switch sync
- **Auth**: JWT-based authentication with `[Authorize]` route guards and browser
  back-button hardening
- **Storage**: Local browser storage for cart persistence and order history
- **Image Handling**: Base64 encoding for embedded menu item images

## Key Features

- **Guided Onboarding**: A 7-step setup wizard (`/welcome → /register → /setup`) walks a
  new tenant through business basics, branding, first outlet, categories, starter menu
  items, and payment methods before landing in the live dashboard. Skippable at any step.
- **Smart Menu**: Categorized menu with popular-item highlights, tag-based search, and
  three switchable layouts (photo cards, editorial list, compact fast-order rows).
- **Dynamic Ordering**: Supports item variants (sizes) and customizable add-ons.
- **Live Order Tracking**: Real-time status bar for customers (Pending → Cooking → Ready).
- **Kitchen Kanban**: Branch-scoped, three-column order management for staff with
  auditory notifications and a dedicated read-only display route for a kitchen monitor.
- **Multi-Branch Operations**: A tenant can run several branches (cafés, food trucks,
  kiosks, bakeries, bistros, restaurants) from one account, switch the "active" branch
  from anywhere in the app, and have Queue, Insights, and Share instantly re-scope to it.
- **Design Studio**: Live-preview branding editor — theme presets, accent color,
  typography, and menu layout — that broadcasts changes across every open circuit
  (admin dashboard and customer menu) with no reload.
- **Admin Tools**: Comprehensive dashboard for menu, category, branch, team, and payment
  management.

## Implementation Step-by-Step

### 1. Foundation & Database
- Initialized the ASP.NET Core Blazor Server project.
- Configured **PostgreSQL** as the production database engine, with an automatic
  **SQLite** fallback so the app boots with zero external setup.
- Defined core entities: `Tenant`, `Outlet`, `MenuItem`, `Category`, `Order`, `OrderItem`,
  and `Addon`, all scoped by `TenantId` for multi-tenant isolation.

### 2. Modern Design System
- Built a reusable UI library in `Shared/UI` (`RollUpModal`, `RollUpInput`,
  `RollUpButton`, `RollUpToggleSwitch`, `WizardStep`, and others).
- Replaced the original "Cafe Espresso" static palette with a semantic token system
  (`--color-bg`, `--color-surface`, `--color-ink`, `--color-accent`, etc.) that a tenant's
  saved branding overrides at runtime, rather than at compile time.
- Applied a dark-by-default aesthetic app-wide, with five selectable theme presets
  (Bistro Classic, Modern Minimal, Rustic Warmth, Clean Slate, Artisan Luxury) and three
  typefaces (Bricolage Grotesque, DM Sans, Fraunces).

### 3. Guided Onboarding & Auth Shell
- Split the app into two layouts: `EmptyLayout` for public/pre-auth pages (`/welcome`,
  `/login`, `/register`, `/setup`) and `MainLayout` for the authenticated workspace.
- Built the 7-step `Setup.razor` wizard on a single component with in-memory state, no
  page reloads between steps, and a Skip option throughout.
- Hardened auth flow: `[Authorize]` on every system page, `AuthorizeRouteView` with
  `RedirectToLogin`, and history-replacing navigation so the browser Back button can't
  return to `/login` after signing in, or to a protected page after signing out.

### 4. Customer Menu & Cart
- Developed a responsive menu with horizontal category scrolling and tag-based search.
- Implemented an intelligent cart system that merges identical items (matching variants
  and add-ons) to keep the order clean.
- Created the `ItemDetailsModal` with sticky headers/footers for a smooth mobile
  experience, restyled for the dark theme.
- Added table-specific ordering routes (`/order/{TableNumber}`) alongside the general
  walk-in menu.

### 5. Real-time Kitchen Queue
- Built a Kanban-style dashboard for staff to manage orders, scoped to the currently
  active branch.
- Integrated **SignalR** to push order updates to staff and customers instantly.
- Added sound alerts for incoming orders (via `IJSRuntime` interop into
  `wwwroot/js/audio.js`, working around Blazor Server's inability to trigger browser
  audio directly).
- Added a dedicated read-only `/display` route for a kitchen monitor/TV.

### 6. Multi-Branch Management
- Extended `Outlet` into a full branch-management feature: add/edit branches, an
  "Active Branch" indicator with a live pulse state, and a branch-info panel showing
  live queue counts, today's orders, and today's revenue per branch.
- Added a global branch switcher in the sidebar (desktop) and header (mobile), with
  `OutletService.OnActiveOutletChanged` propagating the switch to Queue, Insights,
  Manage, and Share without a page reload.
- Enforced strict tenant isolation at the service layer so branches, orders, and
  analytics never cross tenant boundaries.

### 7. Insights & Analytics
- Migrated reporting into `/insights` with Today / This Week / This Month / All Time
  toggles and a per-branch filter.
- Built an SVG revenue/order-count trend chart, a fixed-height peak-rush-hours bar chart
  (fixing an earlier bug where percentage-height bars silently collapsed to zero), and a
  category-share breakdown with a segmented proportional bar.

### 8. Management & Optimization
- Created admin interfaces for full control over menu items, categories, branches, team
  roles, and payment methods.
- Optimized performance using `AsNoTracking` for read-only queries and eager loading
  (`.Include()`) for related data.
- Resolved route collisions (`/queue` and `/menu` each had two competing components
  claiming the same path) by giving staff-facing and customer/display-facing views
  distinct routes.
- Fixed UI constraints (modal scrollability, responsive layout breaks, price-parsing
  locale bugs) to bring the app to production readiness.

## Developer Insights & Architecture Details

As a developer (especially if you're coming from a Python background like
**Flask/Django** or **FastAPI**), here are some specific implementation details that make
this project tick:

### 1. The Dynamic Database Pre-Check
- **The Challenge**: Production apps often fail to start if the external database
  (PostgreSQL) is slightly slow or misconfigured.
- **The Solution**: A **pre-check strategy** in `Program.cs` pings the PostgreSQL server
  at startup. If unreachable, it automatically swaps the EF Core provider to **SQLite**.
- **Developer Tip**: This makes the project "zero-config" for reviewers — it runs
  immediately without a database server, while still preferring PostgreSQL when available.

### 2. The Real-time Engine (SignalR) and Cross-Circuit Broadcasting
- **The Sound**: A clean notification sound plays in the kitchen on every new order.
- **How it works**: Since Blazor runs server-side, it can't directly play sounds in the
  browser. **JSInterop** (`IJSRuntime`) calls a small JavaScript function in
  `wwwroot/js/audio.js`, which triggers the `Audio` object.
- **Beyond orders**: The same "keep every open circuit in sync" pattern now also covers
  branding (`IBrandingService.OnBrandingChanged`) and active-branch state
  (`OutletService.OnActiveOutletChanged`), so a Design Studio change or a branch switch
  is visible on every open tab instantly, not just order status.
- **Developer Tip**: Browsers block auto-playing audio until the user has interacted with
  the page first — the app enables the audio context on that first click.

### 3. Multi-Tenancy and Data Isolation
- Every tenant-owned entity is scoped by `TenantId`, resolved through `ITenantContext`.
- **A real bug worth knowing about**: branch data from one tenant briefly leaked into
  another tenant's dropdown because `MainLayout` queried outlets before authentication
  state (and therefore `CurrentTenantId`) had resolved. The fix — resolving auth state
  first, then scoping every `OutletService` query to `TenantId` — is a good example of
  why tenant scoping needs to happen at the service layer, not just in the UI.

### 4. Data Patterns (Soft Deletes & Eager Loading)
- **Soft Deletes**: Instead of `DELETE`, an `IsDeleted` flag preserves data integrity —
  a production best practice against accidental data loss.
- **Eager Loading**: `.Include(x => x.Category)` avoids the N+1 problem when fetching
  related data.

### 5. Circuit Stability (The Blazor "Gotcha")
- Blazor Server maintains a persistent connection (a "circuit"). An unhandled exception
  in an `async void` method can crash the entire UI for that user.
- **Fix**: Event handlers are wrapped in try/catch, and `InvokeAsync(StateHasChanged)`
  ensures UI updates happen safely from background threads.

### 6. Simple Image Storage
- Rather than standing up an S3-style bucket, images are resized client-side via the
  Canvas API, converted to **Base64 strings**, and stored directly in the database —
  keeping deployment simple and centralized in PostgreSQL.

## How to Run

1. **Prerequisites**: .NET 8 SDK and Node.js installed.
2. **Install & build assets**: `npm install` then `npm run build:css`
3. **Launch**: `dotnet run --project RollUp.csproj`
4. **Explore**:
   - **New tenant**: `http://localhost:5000/welcome`
   - **Demo login**: `http://localhost:5000/login` (`admin@rollup.com` / `Admin123!`,
     or the one-click demo button)
   - **Customer Menu**: `http://localhost:5000/menu/browse`
   - **Kitchen Queue**: `http://localhost:5000/queue`
   - **Kitchen Display (read-only)**: `http://localhost:5000/display`
   - **Admin Panel**: `http://localhost:5000/manage`
   - **Insights**: `http://localhost:5000/insights`
   - **Share (QR & Print)**: `http://localhost:5000/share`
