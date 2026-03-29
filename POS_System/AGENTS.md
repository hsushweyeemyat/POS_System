# POS System Project Instructions

You are helping me build a POS (Point of Sale) Management System in ASP.NET Core MVC using a DATABASE-FIRST approach.

## Core Rules
- Use ASP.NET Core MVC
- Use Entity Framework Core with SQL Server
- Use DATABASE-FIRST only
- Existing SQL schema is the source of truth
- Do NOT convert to code-first
- Use repository + service pattern
- Controllers must stay thin
- Use async/await everywhere

## Performance
- Use AsNoTracking for read queries
- Use projection instead of loading full entities
- Avoid N+1 queries
- Use transactions for checkout

## UI
- Use Bootstrap
- Must be responsive (mobile, tablet, desktop)
- Use cards on mobile instead of tables

## Authentication
- Use Tbl_Users table (NOT ASP.NET Identity tables)
- Use cookie authentication
- Role-based access: Admin, Cashier

## Business Rules
- Validate stock before checkout
- Deduct stock atomically
- Generate invoice after sale
- Admin: manage everything
- Cashier: perform sales + view daily sales