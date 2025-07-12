# e-Parfum .NET API

This is an ASP.NET Core Web API for managing a perfume shop. It provides authentication, user roles, and CRUD operations for perfumes, components, and orders. The API is designed to be consumed by a frontend (such as React).

## Features

- **User Authentication (JWT)**
- **User Roles:** Admin, Supplier, Client
- **Perfume Management:** CRUD for perfumes
- **Component Management:** CRUD for perfume components
- **Order Management:** Standard and custom orders
- **Role-based Dashboard Data**

## Project Structure

- **Controllers:** API endpoints  
  (`Controllers/PerfumesController.cs`, `ComponentsController.cs`, `OrdersController.cs`, `DashboardController.cs`, `Controllers/IdentityUserEndpoints.cs`)
- **Models:** Data models and entities
- **Extensions:** Service and configuration extensions
- **Migrations:** Entity Framework migrations
- **Program.cs:** Main entry point
- **appsettings.json:** Configuration

## Main Tables

| Table                   | Fields                                                                 |
|-------------------------|-----------------------------------------------------------------------|
| Users                   | Id, FullName, Email, UserName, UserRole, ...                          |
| Perfumes                | Id, Name, Description, Price, AvailableQuantity, SupplierId            |
| Components              | Id, Name, Description, PricePerUnit, AvailableQuantity, SupplierId     |
| Orders                  | Id, ClientId, OrderDate, Status, IsCustomOrder, TotalPrice             |
| OrderItems              | Id, OrderId, PerfumeId, Quantity, UnitPrice                            |
| CustomPerfumeOrders     | Id, OrderId, Notes                                                     |
| CustomPerfumeComponents | CustomPerfumeOrderId, ComponentId, Quantity                            |

## API Endpoints

- `/api/IdentityUserEndpoints` - User registration & login
- `/api/Perfumes` - Perfume CRUD
- `/api/Components` - Component CRUD
- `/api/Orders` - Order CRUD
- `/api/Dashboard/{role}` - Dashboard data per role

## Getting Started

1. Clone the repository
2. Configure the database in `appsettings.json`
3. Run migrations to create tables
4. Start the API
5. Consume the API from your React app

## Usage Example

- Register/Login via `/api/IdentityUserEndpoints`
- Fetch perfumes via `/api/Perfumes`
- Place orders via `/api/Orders`
- Get dashboard data via `/api/Dashboard/{role}`

## License

MIT

---

For more details, see the controllers:

- `PerfumesController.cs`
- `ComponentsController.cs`
- `OrdersController.cs`
- `DashboardController.cs`
- `IdentityUserEndpoints.cs`