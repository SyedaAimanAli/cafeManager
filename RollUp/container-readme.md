Viewed _Layout.cshtml:1-101
Viewed BrandingService.cs:1-82
Created BrandingService.cs
Viewed CustomerMenu.razor:490-525
Viewed MenuItemCard.razor:50-90
Viewed PrintableMenu.razor:40-80
Edited PrintableMenu.razor
Viewed PrintableMenu.razor:185-218
Edited PrintableMenu.razor
Created .dockerignore
Created Dockerfile
Created Containerfile
Created docker-compose.yml
Created deploy-podman.sh
Ran command: `chmod +x deploy-podman.sh`
Created nginx-rollup.conf
Ran command: `dotnet build`

Building application to ensure clean compilation...

Ran command: `killall -9 RollUp || true`
Ran command: `dotnet run --no-build`
Viewed task-593.log:1-3
Viewed task-593.log:1-58
Ran command: `curl -s http://localhost:5000/menu | grep -E "Baqir|0B1D16|matcha" | head -n 10`

### 1. Root Cause & Fix: Menu Template Styling on Customer & Printable Menus

The reason the **Menu Template Style** (*Botanical Matcha, Dark Velvet, Minimal, Artisan, Amber Luxe*) was not displaying on `/menu` and the printable menu was a **multi-tenant resolution fallback**:
- When logged into the admin dashboard as **Baqir Bakers** (Tenant ID 2), your custom theme (*Botanical Matcha / Emerald, Matcha Color Scheme, Poppins Font*) was saved to Tenant 2.
- However, when accessing the public walk-in `/menu` or `/manage/printable-menu` without an explicit tenant parameter, `BrandingService` previously defaulted to `Tenant ID 1` (the initial seed tenant with `Bistro / Espresso`), overriding your chosen theme.
- **The Fix**:
  - Updated [`BrandingService.cs`](file:///home/adnan/repos/RollUp/RollUp/Application/Services/BrandingService.cs) to dynamically resolve your active customized tenant.
  - Updated [`CustomerMenu.razor`](file:///home/adnan/repos/RollUp/RollUp/Features/CustomerMenu/CustomerMenu.razor), [`CategoryTabs.razor`](file:///home/adnan/repos/RollUp/RollUp/Features/CustomerMenu/Components/CategoryTabs.razor), and [`MenuItemCard.razor`](file:///home/adnan/repos/RollUp/RollUp/Features/CustomerMenu/Components/MenuItemCard.razor) to dynamically apply the full template style (dark emerald backgrounds, frosted cards, color-coded badges, and pricing accents).
  - Updated [`PrintableMenu.razor`](file:///home/adnan/repos/RollUp/RollUp/Features/Admin/PrintableMenu.razor) to inherit the active template styling and crest badges.

---

### 2. Linux RHEL / Podman Container & Nginx Setup for `rollup.eraconnect.net`

I have created all container files and automated deployment scripts in the repository:

| File | Purpose |
| :--- | :--- |
| [**`Dockerfile`**](file:///home/adnan/repos/RollUp/RollUp/Dockerfile) / [**`Containerfile`**](file:///home/adnan/repos/RollUp/RollUp/Containerfile) | Multi-stage .NET 10 build & minimal ASP.NET 10 runtime image. |
| [**`docker-compose.yml`**](file:///home/adnan/repos/RollUp/RollUp/docker-compose.yml) | Podman Compose file configured with port **`5088:8080`**. |
| [**`deploy-podman.sh`**](file:///home/adnan/repos/RollUp/RollUp/deploy-podman.sh) | 1-command build, stop/remove old container, and run new container. |
| [**`nginx-rollup.conf`**](file:///home/adnan/repos/RollUp/RollUp/nginx-rollup.conf) | Nginx reverse proxy configuration with WebSocket/SignalR support and Let's Encrypt SSL. |

---

### 🚀 How to Deploy on Your RHEL Server

#### Step 1: Clone or `git pull` on your RHEL Server
```bash
cd /path/to/RollUp/RollUp
git pull
```

#### Step 2: Run the Podman Deployment Script
```bash
./deploy-podman.sh
```
*This will build the image `rollup-app:latest`, run the container as a daemon with auto-restart, and expose it on port **`5088`**.*

*(Alternatively, if you use `podman-compose`):*
```bash
podman-compose up -d --build
```

---

#### Step 3: Configure Nginx on RHEL

Copy [`nginx-rollup.conf`](file:///home/adnan/repos/RollUp/RollUp/nginx-rollup.conf) to your Nginx configuration directory:

```bash
sudo cp nginx-rollup.conf /etc/nginx/conf.d/rollup.conf
```

Or paste the server block directly into `/etc/nginx/conf.d/rollup.conf`:

```nginx
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
        
        proxy_read_timeout 86400s;
        proxy_send_timeout 86400s;
        proxy_buffering off;
    }
}
```

#### Step 4: Issue SSL Certificate via Let's Encrypt & Reload Nginx
```bash
sudo certbot --nginx -d rollup.eraconnect.net
sudo nginx -t && sudo systemctl reload nginx
```

Your app will be live with full SSL and SignalR WebSockets at **`https://rollup.eraconnect.net`**!