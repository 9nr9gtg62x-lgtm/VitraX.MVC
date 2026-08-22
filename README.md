# VitraX.MVC

ASP.NET Core MVC front-end for **VitraX** — a glass factory production management system.
Consumes the same database as [`VitraX.API`](../VitraX.API) via `AppDbContext` and provides
a server-rendered dashboard for staff to manage products, workers, production orders, and
production tasks.

## Stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core (SQL Server LocalDB)
- Cookie authentication (`AccountController`)
- Bootstrap 5 (RTL) + Bootstrap Icons + DataTables.js + Bootstrap Modal
- Cairo / Inter (Google Fonts)

## Structure

- `Controllers/` — one controller per entity (`Products`, `Workers`, `ProductionOrders`,
  `ProductionTasks`) plus `AccountController` (login/register/logout) and `HomeController`
  (dashboard).
- `Views/` — Index / Details / Create / Edit / Delete per entity, `Views/Shared/_Layout.cshtml`
  for the shell (sidebar, top bar, footer).
- `wwwroot/css/vitrax.css` — brand tokens (CSS variables) and the handful of custom classes
  Bootstrap doesn't provide out of the box (glass-card effect, sidebar, status badges).

## Running locally

```
dotnet run
```

Requires the same `DefaultConnection` LocalDB database as `VitraX.API` (see `appsettings.json`).
