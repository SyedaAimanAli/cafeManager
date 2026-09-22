# ☕ RollUp - Multi-Tenant Cafe Management Platform

A modern, high-performance cafe operations and customer ordering platform built with **Blazor Server** and **ASP.NET Core 10 / PostgreSQL**. Features real-time kitchen queues, table QR ordering, dynamic branding design studios, printable menus, and detailed analytics.

![License](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)
![Container](https://img.shields.io/badge/Container-Podman%20%7C%20Docker-purple?logo=podman)

---

## 🚀 Production Deployment Guide (RHEL / Podman / Nginx)

This repository includes automated container configurations and deployment scripts for Linux RHEL servers using **Podman**, **Nginx reverse proxy**, and **PostgreSQL**, exposed on port **`5088`** for domain **`rollup.eraconnect.net`**.

### 1. Clone & Quick Deploy with Podman
```bash
git clone https://github.com/your-username/RollUp.git
cd RollUp/RollUp

# Run the automated deployment script (builds image, stops previous container, runs on port 5088)
chmod +x deploy-podman.sh
./deploy-podman.sh
```

*(Alternatively with podman-compose)*:
```bash
podman-compose up -d --build
```

---

### 2. Configure Environment & Database (PostgreSQL)
To customize database credentials, pass environment variables to `deploy-podman.sh` or edit `docker-compose.yml`:
```bash
export DB_HOST="127.0.0.1"      # or host.containers.internal
export DB_PORT="5432"
export DB_NAME="rollup"
export DB_USER="postgres"
export DB_PASS="your_secure_password"

./deploy-podman.sh
```

---

### 3. Configure Nginx Reverse Proxy (`rollup.eraconnect.net`)
Copy the provided [`nginx-rollup.conf`](file:///home/adnan/repos/RollUp/RollUp/nginx-rollup.conf) to your Nginx configuration directory:

```bash
sudo cp nginx-rollup.conf /etc/nginx/conf.d/rollup.conf
```

**Nginx Configuration (`/etc/nginx/conf.d/rollup.conf`)**:
```nginx
# WebSocket / SignalR Connection Upgrade
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      close;
}

server {
    listen 80;
    server_name rollup.eraconnect.net;

    location / {
        proxy_pass http://127.0.0.1:5088;
        
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
        
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # SignalR long-lived WebSocket timeout
        proxy_read_timeout 86400s;
        proxy_send_timeout 86400s;
        proxy_buffering off;
    }
}
```

---

### 4. Enable SSL with Let's Encrypt
```bash
sudo certbot --nginx -d rollup.eraconnect.net
sudo nginx -t && sudo systemctl reload nginx
```
Your app is now securely live at **`https://rollup.eraconnect.net`**!

---

## 📖 Overview

RollUp is a full-stack cafe management platform that streamlines the entire customer experience—from browsing a dynamic menu to real-time order tracking—while providing kitchen staff with an efficient Kanban-style dashboard for order management.

### Key Highlights
-  **Real-time Updates** using SignalR for instant order tracking
-  **Responsive Design** with Tailwind CSS custom design system
-  **Modern UI Components** with "Cafe Espresso" aesthetic
-  **Smart Notifications** with audio alerts for incoming orders
-  **Persistent Storage** using Entity Framework Core + PostgreSQL

##  Features

### Customer Experience
- **Smart Menu Display** - Categorized items with popular highlights and tag-based search
- **Dynamic Ordering** - Support for item variants (sizes) and customizable add-ons
- **Intelligent Cart** - Automatically merges identical items for a clean checkout
- **Live Order Tracking** - Real-time status updates (Pending → Cooking → Ready)
- **Order History** - Local browser storage for cart persistence

### Kitchen Operations
- **Kanban Dashboard** - Visual order management with drag-and-drop status updates
- **Audio Alerts** - Sound notifications for incoming orders
- **Real-time Queue** - Instant synchronization across all kitchen displays
- **Queue Analytics** - Monitor order processing times and queue status

### Admin Management
- **Menu Management** - Add, edit, and organize menu items and categories
- **Outlet Control** - Manage multiple cafe outlets
- **Category Organization** - Group items with custom tags and filters
- **System Configuration** - Configure payment methods and subscription plans

##  Tech Stack

| Component | Technology |
|-----------|-----------|
| **Frontend Framework** | Blazor Server (ASP.NET Core 8) |
| **Real-time** | SignalR WebSockets |
| **Styling** | Tailwind CSS v3.4 |
| **Database** | PostgreSQL |
| **ORM** | Entity Framework Core 8 |
| **Architecture** | Clean Architecture with Repository Pattern |

##  Project Structure

```
RollUp/
├── API/                          # REST API endpoints
│   ├── Controllers/              # MenuController, OrdersController, etc.
│   ├── Hubs/                     # SignalR hubs for real-time updates
│   └── Middleware/               # Exception handling & cross-cutting concerns
│
├── Application/                  # Application layer (DTOs & Services)
│   ├── DTOs/                     # Data transfer objects
│   └── Services/                 # Business logic & domain services
│
├── Core/                         # Domain layer
│   ├── Entities/                 # Database models (Order, MenuItem, etc.)
│   ├── Enums/                    # Domain enumerations
│   ├── Interfaces/               # Service contracts
│   ├── Models/                   # Domain models
│   └── Services/                 # Core business logic
│
├── Infrastructure/               # Infrastructure layer
│   ├── Authentication/           # JWT token provider
│   ├── Persistence/              # Database context & migrations
│   └── Repositories/             # Generic repository pattern
│
├── Features/                     # Razor components (Pages & UI)
│   ├── Admin/                    # Admin management pages
│   ├── CustomerMenu/             # Customer menu browsing
│   └── Queue/                    # Kitchen queue display
│
├── Shared/                       # Shared components
│   ├── Layouts/                  # MainLayout component
│   └── UI/                       # Reusable UI components (RollUpButton, etc.)
│
├── Pages/                        # Razor pages (_Host.cshtml, Error pages)
├── Styles/                       # Global CSS
├── wwwroot/                      # Static assets (CSS, JS)
├── Migrations/                   # EF Core database migrations
└── Program.cs                    # Application startup configuration
```

##  Architecture

RollUp follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────┐
│   Presentation (Razor Pages)    │
├─────────────────────────────────┤
│   Application (DTOs, Services)  │
├─────────────────────────────────┤
│   Core (Entities, Interfaces)   │
├─────────────────────────────────┤
│  Infrastructure (DB, Repos)     │
└─────────────────────────────────┘
```

### Key Patterns
- **Repository Pattern** - Abstract data access layer
- **Dependency Injection** - Loose coupling via service container
- **Service Layer** - Centralized business logic
- **Entity Framework** - ORM for database operations
- **SignalR Hubs** - Real-time communication channels

##  Getting Started

### Prerequisites
- **.NET 8 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Node.js** (for npm dependencies)
- **PostgreSQL** (Required for database operations)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/SyedaAimanAli/RollUp.git
   cd RollUp
   ```

2. **Install dependencies**
   ```bash
   cd RollUp
   npm install
   dotnet restore
   ```

3. **Configure database connection**
   
   Edit `RollUp/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=rollup;User Id=postgres;Password=yourpassword;"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run --project RollUp.csproj
   ```

6. **Access the application**
   -  Customer Menu: `http://localhost:5000`
   -  Admin Panel: `http://localhost:5000/kitchen`
   -  Kitchen Queue: `http://localhost:5000/queue`

## 🔌 API Endpoints

### Authentication
```
POST   /api/auth/register      - Register new user
POST   /api/auth/login         - Login user
POST   /api/auth/logout        - Logout user
```

### Menu Management
```
GET    /api/menu               - Get all menu items
GET    /api/menu/{id}          - Get menu item details
POST   /api/menu               - Create menu item (Admin)
PUT    /api/menu/{id}          - Update menu item (Admin)
DELETE /api/menu/{id}          - Delete menu item (Admin)
```

### Orders
```
GET    /api/orders             - Get user orders
POST   /api/orders             - Create new order
GET    /api/orders/{id}        - Get order details
PUT    /api/orders/{id}        - Update order status
GET    /api/orders/{id}/status - WebSocket for real-time updates
```

### Queue Management
```
GET    /api/queue              - Get current queue
PUT    /api/queue/{id}/status  - Update order status (Kitchen)
GET    /api/queue/stats        - Get queue statistics
```

##  Developer Insights

### Real-time Engine (SignalR)
SignalR manages WebSocket connections for instant updates across all connected clients. When an order status changes, notifications are broadcast to customers and kitchen staff simultaneously.

**Key Files:**
- [OrderHub.cs](RollUp/API/Hubs/OrderHub.cs) - Order update hub
- [QueueHub.cs](RollUp/API/Hubs/QueueHub.cs) - Queue status hub

### Audio Notifications
Audio alerts use JavaScript interop to trigger browser notifications. The system waits for user interaction to enable audio due to browser autoplay restrictions.

**Files:**
- [audio.js](RollUp/wwwroot/js/audio.js) - Audio playback handler
- [OrderNotificationService.cs](RollUp/Core/Services/OrderNotificationService.cs)

### Image Storage
Menu item images are stored as Base64 strings in the database, eliminating the need for external storage services while keeping the system self-contained.

### Performance Optimizations
- **AsNoTracking** - Used for read-only queries to reduce memory overhead
- **Eager Loading** - `.Include()` prevents N+1 database problems
- **Soft Deletes** - `IsDeleted` flag preserves data integrity

##  Testing

Run unit tests:
```bash
dotnet test
```

##  Database Schema

### Core Tables
- **Users** - Customer and staff accounts
- **MenuItems** - Cafe menu items with pricing
- **Categories** - Menu item categorization
- **Orders** - Customer orders with timestamps
- **OrderItems** - Individual items within orders
- **OrderAddons** - Customizations (extra toppings, etc.)
- **Payments** - Payment transaction records
- **QueueEntries** - Real-time kitchen queue

##  Contributing

Contributions are welcome! Here's how to get started:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- Code follows C# naming conventions
- Commits have clear, descriptive messages
- New features include appropriate error handling
- Database changes include migrations

##  Documentation

- [Project Summary](RollUp/ProjectSummary.md) - Detailed technical breakdown
- [Architecture Decisions](docs/ARCHITECTURE.md) - Design rationale (coming soon)
- [API Documentation](docs/API.md) - Comprehensive endpoint reference (coming soon)

##  Troubleshooting

### SignalR Connection Issues
- Ensure WebSockets are enabled on your hosting environment
- Check firewall settings allow persistent connections
- Verify SignalR hub endpoints are correctly mapped in `Program.cs`

### Database Connection Errors
- Verify PostgreSQL is running and accessible
- Check connection string in `appsettings.json`
- Run `dotnet ef database update` to apply migrations

### Audio Not Playing
- Ensure browser hasn't muted the tab
- Check browser console for JavaScript errors in `audio.js`
- Verify user has interacted with the page before audio playback

##  License

This project is licensed under the MIT License - see the LICENSE file for details.

##  Author

**Aiman Ali**
- GitHub: [@SyedaAimanAli](https://github.com/SyedaAimanAli)
- Email: aimanali122007@gmail.com

##  Acknowledgments

- ASP.NET Core team for excellent documentation
- SignalR for real-time capabilities
- Tailwind CSS for utility-first styling
- Entity Framework Core team for powerful ORM features

---

**Made with ☕ by Aiman Ali**

Have questions or suggestions? Feel free to open an issue or reach out!
