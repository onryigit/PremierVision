# PremierVision

PremierVision is an ASP.NET Core web application for tracking Premier League fixtures, live matches, match details, standings, and admin-managed match data. The solution is split into a backend API and an MVC frontend so the UI consumes structured football data through HTTP endpoints instead of querying the database directly.

## Features

- Weekly Premier League match overview
- Live, completed, and upcoming match sections on the home page
- Featured match card with live minute display
- Fixture list grouped by match day and match week
- Match detail page with score, timeline events, goals, cards, substitutions, and statistics
- Standings table calculated from completed fixtures
- Admin panel for adding fixtures, match events, and match statistics
- SQL Server persistence with Entity Framework Core
- Responsive MVC UI styled with Bootstrap, Bootstrap Icons, and custom CSS

## Tech Stack

- .NET 10
- ASP.NET Core MVC
- ASP.NET Core Web API
- Entity Framework Core 10
- SQL Server
- Bootstrap
- Bootstrap Icons

## Solution Structure

```text
PremierVision/
+-- PremierVision.API/        # Backend API, EF Core DbContext, domain models, controllers
+-- PremierVision/            # MVC frontend application
+-- PremierVision.slnx        # Solution file
`-- README.md
```

## Projects

### PremierVision.API

Backend service responsible for database access and football data operations.

Main endpoints:

```text
GET  /api/home
GET  /api/home?week=38
GET  /api/fixtures
GET  /api/fixtures?week=38
GET  /api/matches/{id}
GET  /api/standings
GET  /api/admin/options
POST /api/admin/fixtures
POST /api/admin/events
POST /api/admin/statistics
```

### PremierVision.UI

MVC frontend that consumes the API through `PremierVisionApiClient`.

Main pages:

```text
/                 Home page
/index.html       Home page alias
/fixtures.html    Fixture list
/standings.html   Standings table
/match-detail.html/{id}
/admin
```

## Prerequisites

- .NET 10 SDK
- SQL Server
- Visual Studio, Visual Studio Code, or any .NET-compatible editor

## Configuration

The API connection string is stored in:

```text
PremierVision.API/appsettings.json
```

Current example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SQL_SERVER;Initial Catalog=PremierVisionDb;Integrated Security=True;Trust Server Certificate=True;Encrypt=False;MultipleActiveResultSets=True;"
  }
}
```

Update `Data Source` for your local SQL Server instance before running the project on another machine.

The UI points to the API through:

```text
PremierVision/appsettings.json
```

```json
{
  "PremierVisionApi": {
    "BaseUrl": "http://localhost:5102/"
  }
}
```

## Running Locally

Restore and build:

```bash
dotnet restore
dotnet build PremierVision.API/PremierVision.API.csproj
dotnet build PremierVision/PremierVision.UI.csproj
```

Start the API:

```bash
dotnet run --project PremierVision.API/PremierVision.API.csproj --urls http://localhost:5102
```

Start the MVC UI in a second terminal:

```bash
dotnet run --project PremierVision/PremierVision.UI.csproj --urls http://localhost:5059
```

Open:

```text
http://localhost:5059
```

## Database Notes

The application expects an existing SQL Server database named `PremierVisionDb` with these main tables:

- `Teams`
- `Fixtures`
- `MatchEvents`
- `MatchStatistics`

Entity relationships and table mapping are defined in:

```text
PremierVision.API/Data/AppDbContext.cs
```

Standings are calculated dynamically from completed fixtures by:

```text
PremierVision.API/Services/StandingsService.cs
```

## Development Notes

- Stop running API/UI processes before rebuilding in Visual Studio. Otherwise Windows can lock `PremierVision.API.exe` or `PremierVision.UI.exe` and cause `MSB3021` / `MSB3027` copy errors.
- The frontend depends on the API being available at the configured `PremierVisionApi:BaseUrl`.
- Local `.dotnet`, `.dotnet-home`, `bin`, `obj`, `.vs`, and log files should not be committed.

## License

This project is currently provided for portfolio and educational use. Add a license file before publishing it as an open-source project.
