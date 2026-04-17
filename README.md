# FamilyPasswordManager

A secure password manager built with ASP.NET Core, Javascript and Razor Pages. Store passwords, payment cards, and security keys and share them with trusted family members,friends or colleagues.

## Features

Secured Vault — Store Encrypted web logins, credit/debit cards, and security keys, with extra pin layer for security
Folder Organization — Group credentials into named folders
My items tab — A personal list of all your individually saved credentials
Item-Level sharing — Share individual items with other users with View or Edit permissions and expiry dates
Folder-Level sharing — Share entire folders with family members, friends or colleagues
Edit history — Every change is tracked with who edit it, easily audited
Shared item tab — See everything shared with you in one place, with history and edit access
Delete account — Full account deletion with safe cleanup of personal data and all shared access records

##Stack

- **Backend** — ASP.NET Core, Razor Pages, EF Core, SQL Server
- **Auth** — ASP.NET Identity with a custom 5-digit vault PIN
- **Frontend** — Vanilla JS, custom dark-theme CSS 
- **API** — RESTful controllers for all CRUD and access operations

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server Express (or full SQL Server)
- Visual Studio 2022 (recommended)

### Setup

1. **Clone the repo**

2. **Configure the database** — update `appsettings.json` with your connection string:

3. **Run migrations** — in Visual Studio Package Manager Console:

4. **Run the app**

## Project Structure

```
FPasswordManager/
├── Areas/
│   └── Identity/
│       └── Pages/         # Login, register, manage profile
├── Controllers/           # API controllers (credentials, access, items)
├── Data/
│   └── AppDbContext.cs    # EF Core context
├── Models/                # Entity models
├── Pages/                 # Razor Pages (dashboard, vault, shared items)
├── Services/              # Business logic (item CRUD, access, sharing)
├── wwwroot/
│   ├── css/               # site.css — full dark theme
│   └── js/                # index.js, vault.js, shareditems.js
├── appsettings.json
└── Program.cs
```


## Security Notes

- All vault access is gated behind a 5-digit PIN verified on every visit
- Passwords, card numbers, CVV, and PINs are encrypted at rest
- Shared access has mandatory expiry dates with a default 1 year expiry
- Account deletion cascades and cleans up all access records safely


## Team Members
-Oak Soe Khant(Developer)
