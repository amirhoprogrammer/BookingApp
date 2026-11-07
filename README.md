# BookingApp

A sports court booking web application built with **ASP.NET Core**, **Blazor Server**, **Entity Framework Core** and **ASP.NET Core Identity**.

Users can register, browse courts, see the available time slots for a given day and book one. The application prevents double bookings at the database level, so two users can never reserve the same slot, even if they click at the same moment.

## Features

- **Authentication:** register, log in and log out with ASP.NET Core Identity
- **Courts:** list of courts with price per hour
- **Availability:** pick a court and a date to see only the free time slots (past slots are hidden)
- **Booking:** book a slot as a logged-in user; anonymous users are redirected to the login page and returned to the booking page afterwards
- **Concurrency protection:** a filtered unique index on active bookings guarantees one confirmed booking per time slot
- **Business rules in a service layer:** no booking in the past, cancellation allowed only 24 hours before the start time, and users can only cancel their own bookings
- **Seed data:** sample courts and hourly time slots for the next 7 days are created on first run

## Tech Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core, Blazor Server (Interactive Server render mode) |
| Language | C# |
| Database | SQLite |
| ORM | Entity Framework Core (code-first migrations) |
| Auth | ASP.NET Core Identity |
| UI | Bootstrap (from the Blazor template) |

## Architecture Overview

```
BookingApp/
├── Components/          Blazor pages, layout and Identity UI
│   └── Pages/Courts.razor   Court selection and booking page
├── Data/                Entities, DbContext, migrations and seeder
│   ├── ApplicationUser.cs
│   ├── Court.cs
│   ├── TimeSlot.cs
│   ├── Booking.cs
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs
├── Services/
│   └── BookingService.cs    Booking and cancellation logic
└── Program.cs               Dependency injection and app startup
```

### Data model

- `ApplicationUser` extends `IdentityUser` with `FullName`
- `Court` has many `TimeSlot`
- `TimeSlot` has many `Booking` (only one can be `Confirmed`)
- `Booking` links a user to a time slot with a status (`Confirmed` or `Cancelled`)

### Key design decisions

**Preventing double bookings.** Checking availability in code is not enough, because two requests can pass the check at the same time. A filtered unique index on `Booking.TimeSlotId` (only where `Status = Confirmed`) lets the database reject the second insert. The service catches the resulting `DbUpdateException` and shows a friendly message. Cancelled bookings are excluded from the index, so a cancelled slot can be booked again.

**`IDbContextFactory` in Blazor Server.** Blazor Server keeps a long-lived circuit per user, so injecting a single `DbContext` into components can cause concurrency problems. `BookingService` uses `IDbContextFactory<ApplicationDbContext>` and creates a short-lived context per operation.

**Business logic outside components.** Rules like the 24-hour cancellation window live in `BookingService`, which keeps the Razor components thin and the logic easy to test.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (the version used by the project template)
- `dotnet-ef` tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Run locally

```bash
git clone https://github.com/<your-username>/BookingApp.git
cd BookingApp/BookingApp

dotnet ef database update
dotnet run
```

Then open the URL printed in the terminal (for example `http://localhost:5129`). Register a new account and go to **Courts** to make a booking.

The SQLite database file (`Data/app.db`) is created locally and is not committed to the repository.

## Screenshots

> Add screenshots here, for example `docs/courts-page.png`, then reference them:
>
> `![Courts page](docs/courts-page.png)`

## Roadmap

- [ ] "My Bookings" page with cancel button (the cancellation logic already exists in `BookingService`)
- [ ] User profile page
- [ ] Admin role and panel to manage courts and time slots
- [ ] Unit tests for `BookingService` (xUnit)
- [ ] Email confirmation for bookings
- [ ] Docker support and deployment

## What I Learned

- Using ASP.NET Core Identity with a custom user class
- Designing a code-first database with EF Core migrations
- Handling race conditions with database constraints instead of only application checks
- Working with dependency injection and `IDbContextFactory` in Blazor Server
- Keeping business rules in a service layer

## License

This project is for learning and portfolio purposes.
