# API Documentation - RollUp

RollUp provides a RESTful API layer alongside its Blazor interface for potential integration with mobile apps or third-party services.

## 🔑 Authentication
- **Endpoint**: `/api/auth/login`
- **Method**: `POST`
- **Body**: `{ "email": "...", "password": "..." }`
- **Response**: JWT Token

## 📋 Menu Endpoints
| Endpoint | Method | Description | Auth Required |
|----------|--------|-------------|---------------|
| `/api/menu` | GET | Retrieve all active menu items | No |
| `/api/menu/{id}` | GET | Get details for a specific item | No |
| `/api/menu` | POST | Create a new menu item | Yes (Admin) |
| `/api/menu/{id}` | PUT | Update an existing item | Yes (Admin) |

## 🛍 Order Endpoints
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/orders` | POST | Submit a new order |
| `/api/orders/{id}` | GET | Check order status |
| `/api/orders/history` | GET | Retrieve order history for current user |

## ⏱ Real-time Hubs (SignalR)
- **Queue Hub**: `/hubs/queue` - Real-time kitchen staff updates.
- **Order Hub**: `/hubs/orders` - Instant status updates for customers.

## 🛠 Developer Notes
- All endpoints return standard JSON.
- Error states follow standard HTTP status codes (400, 401, 403, 404, 500).
- Timestamps are returned in UTC (ISO 8601).
