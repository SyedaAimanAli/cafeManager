# Architecture Decisions - RollUp

This document outlines the architectural patterns and design decisions made for the RollUp Cafe Management System.

## 🏛 Clean Architecture
The project follows a modified Clean Architecture pattern to ensure separation of concerns and maintainability:

1.  **Core**: Contains domain entities, interfaces, and business logic. No dependencies on outer layers.
2.  **Infrastructure**: Implementation of data persistence (EF Core), authentication (JWT), and external services.
3.  **Application**: Service layer that orchestrates domain logic and DTO transformations.
4.  **Presentation (Blazor Features)**: Feature-based organization of Razor components and UI logic.

## 🔄 Real-time Communication
- **SignalR**: Chosen over traditional polling to provide instant updates for order status changes and kitchen notifications. This reduces server load and provides a "premium" feel.
- **Hub Strategy**: Separate hubs for `Orders` and `Queue` to keep communication channels focused and efficient.

## 💾 Database Strategy (Dynamic Fallback)
- **Primary**: PostgreSQL is the target production database for high performance and relational integrity.
- **Stability Fallback**: A custom **Pre-Check Strategy** in `Program.cs` pings the database at startup. If unreachable, the system transparently swaps to **SQLite**. This ensures the application remains functional in diverse environments without manual configuration.

## 🎨 UI & UX Design
- **Feature-Based UI**: Razor components are grouped by feature (e.g., `Features/Queue`, `Features/Admin`) rather than type. This makes the codebase easier to navigate as it grows.
- **Tailwind CSS**: Used for rapid, consistent styling without the overhead of heavy UI frameworks.
- **JS Interop**: Leveraged specifically for browser-level features like audio playback and local storage persistence.

## 🛡 Security & Resilience
- **Soft Deletes**: All critical entities use an `IsDeleted` flag combined with EF Core Global Query Filters.
- **Circuit Breakers**: Event handlers in Blazor are wrapped in try-catch blocks to prevent UI crashes during background processing.
