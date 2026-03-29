# POS System

## Overview

This project is an ASP.NET Core MVC Point of Sale (POS) system for day-to-day retail counter operations. It supports role-based login, product browsing, cart-based sales checkout, invoice viewing, sales history, product management, category management, and user management.

The current default landing screen is the sales dashboard, so cashiers can start a transaction immediately after sign-in.

## Main Modules

- Sales dashboard / main screen
- Cart and checkout flow
- Sale history and invoice view
- Product management
- Category management
- User management
- Role-based access for Admin and Cashier

## Tech Stack

### Backend

- .NET 8
- ASP.NET Core MVC
- ASP.NET Core Identity Core with cookie authentication
- Session-based cart handling
- Entity Framework Core 8
- SQL Server

### Frontend

- Razor Views (`.cshtml`)
- Bootstrap
- JavaScript
- jQuery
- Custom CSS under `wwwroot/css`

### Project Pattern

The codebase follows a layered structure:

- `Controllers` for HTTP endpoints and screen flow
- `Services` for business logic
- `Repositories` for data access
- `ViewModels` for UI binding
- `Data` for EF Core context and entities

## Database

For the database structure and master data, use:

- [POS_System/Docs/POS_System.sql](POS_System/Docs/POS_System.sql)

This SQL file is the reference for:

- Table structure
- Seed / master data
- Initial records used by the system

## Default Login Accounts

### Admin

- Email: `admin1@gmail.com`
- Password: `Admin123`

### Cashier

- Email: `cashier1@gmail.com`
- Password: `Admin123`

## Configuration

The default SQL Server connection string is stored in:

- `POS_System/appsettings.json`

Update it if your SQL Server instance, database name, or credentials are different.

## How To Run

1. Create or restore the database using [POS_System/Docs/POS_System.sql](POS_System/Docs/POS_System.sql).
2. Verify the connection string in `POS_System/appsettings.json`.
3. Open `POS_System.sln` in Visual Studio or run the project with `dotnet run` from the project folder.
4. Sign in with one of the accounts listed above.

## Notes

- Admin users can access all management screens.
- Cashier users can access sales-related screens only.
- For production use, change the default login credentials and database credentials before deployment.
