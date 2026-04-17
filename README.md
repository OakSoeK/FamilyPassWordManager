# FamilyPasswordManager

A secure password manager built with ASP.NET Core, Javascript and Razor Pages. Store passwords, payment cards, and security keys and share them with trusted family members,friends or colleagues.

## Features

Secured Vault — Store Encrypted web logins, credit/debit cards, and security keys, with extra pin layer for security

Folder Organization — Group credentials into named folders

My items tab — A list of all your individually saved credentials

Item-Level sharing — Share individual items with other users with View or Edit permissions and expiry dates

Folder-Level sharing — Share entire folders with family members, friends or colleagues

Edit history — Every change is tracked with who edit it, easily audited

Shared item tab — See everything shared with you in one place, with history and edit access

Delete account — Full account deletion with safe cleanup of personal data and all shared access records

## Stack

- **Backend** — ASP.NET Core, Razor Pages, EF Core, SQL Server
- **Auth** — ASP.NET Identity with a custom 5-digit vault PIN and Fields
- **Frontend** — Vanilla JS, custom dark-theme CSS 
- **API** — RESTful controllers for all CRUD and access operations

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server Express (or full SQL Server)
- Visual Studio 2022 (recommended)

### Setup

1. **Clone the repo**

2. **Configure database** — update `appsettings.json` with your connection string

3. **Run migrations** — in Visual Studio Package Manager Console
   ``Update-Database``

4. **Run the app**

## Project Structure

```
FPassWordManager/
├── Areas/
│   └── Identity/
│       └── Pages/                                       # Profile, password, PIN, delete (Custom-Edit Account)
│           └── Account/
│               ├── Login.cshtml / Login.cshtml.cs
│               ├── Logout.cshtml / Logout.cshtml.cs
│               ├── Register.cshtml / Register.cshtml.cs
│               └── Manage/
│                   └── Index.cshtml / Index.cshtml.cs   
├── Controllers/
│   └── Controllers.cs                                   # API controllers (credentials, access, items)
├── Data/
│   └── AppDbContext.cs                                  # EF Core context
├── DTOs/                                                # Data transfer objects
├── Extensions/
│   └── Extensions.cs                                    # Service registration helpers
├── Migrations/                                          # Migration files
├── Models/
│   └── Models.cs                                        # Models
├── Pages/
│   ├── Shared/                                          # Layout and partials
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Error.cshtml / Error.cshtml.cs
│   ├── Index.cshtml / Index.cshtml.cs                   # Dashboard
│   ├── Privacy.cshtml / Privacy.cshtml.cs
│   ├── SharedItems.cshtml / SharedItems.cshtml.cs       # Items shared with you
│   └── Vault.cshtml / Vault.cshtml.cs                   # Folder, Vault UI
├── Services/
│   ├── Interfaces/
│   │   └── Interfaces.cs                                # Service interfaces
│   └── Services.cs                                      # Main Logic
├── wwwroot/
│   ├── css/
│   │   ├── bootstrap.css
│   │   └── site.css                                     # Custom dark theme
│   └── js/
│       ├── index.js                                     # Dashboard logic
│       ├── shareditems.js                               # Shared items logic
│       ├── site.js
│       └── vault.js                                     # Vault logic
├── appsettings.json
├── launchSettings.json
└── Program.cs
```


## Security Notes

- All vault access is gated behind a 5-digit PIN verified on every visit
- Passwords, card numbers, CVV, and PINs are encrypted at rest
- Shared access has mandatory expiry dates with a default 1 year expiry
- Account deletion cascades and cleans up all access records safely


## Team Members


-Oak Soe Khant(Developer)
