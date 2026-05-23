# Fasally Backend

Fasally is a backend API for a tailoring marketplace. It supports user authentication, role-based access, tailor profiles, categories, and portfolio browsing.

## Features

- Email/password authentication
- Google login
- JWT access tokens and refresh tokens
- Email confirmation and password reset
- User, role, and permission management
- Tailor profile approval flow
- Tailor category management
- Tailor browsing and portfolio management

## Tech Stack

- ASP.NET Core
- Entity Framework Core
- ASP.NET Core Identity
- SQL Server
- JWT authentication
- Serilog
- Mapster
- FluentValidation

## Getting Started

```bash
dotnet restore
dotnet build
dotnet run
```

## API Areas

- Auth: `/Auth`
- Account: `/me`
- Users: `/api/Users`
- Roles: `/api/Roles`
- Categories: `/api/Categories`
- Tailors: `/api/Tailors`
- Tailor browsing: `/api/TailorBrowsing`
