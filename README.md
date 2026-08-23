# Digital Forms System

A fullstack internal web application built for **HS Technologies (Phils.), Inc. (HST)**, developed during a 600-hour internship in the MIS-IT section. The system digitizes two core company processes — **Fixed Asset Requests** and **Damaged Reports** — replacing manual paper-based workflows with a role-based, database-driven web application.

> ⚠️ This repository is a personal portfolio continuation of an internship deliverable. It does not contain any real company data. All employee records, credentials, and department data used for local development/testing are synthetic/dummy data.

---

## ✨ Features

- **Role-based access control (RBAC)** — separate views and permissions for IT Manager and Employee roles
- **Session-based authentication** with BCrypt password hashing
- **Temporary password onboarding flow** for first-time logins
- **Fixed Asset Request module** — submit, track, and manage asset requests
- **Damaged Report module** — report and manage damaged equipment/tools
- **PDF generation** for reports (via QuestPDF)
- **Audit logging** for administrative actions

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | C# / ASP.NET Core MVC (.NET 8.0) |
| Database | SQL Server (Entity Framework Core 8.0.0) |
| Auth | Session-based auth, BCrypt.Net-Next |
| PDF Generation | QuestPDF |
| Hosting | IIS (ASP.NET Core Hosting Bundle + In-Process hosting) |
| Architecture | Being refactored into a Shared Class Library + Blazor Server structure |

---

## 🏗️ Architecture

The system follows a layered architecture:

```
Presentation Layer   →  Razor Views, CSS/JS, PDF Export (QuestPDF)
Controller Layer     →  Home, Account, FixedAsset, Damaged, Reports
Service Layer        →  ICurrentUserService, IFixedAssetRequestService,
                         IDamagedReportService, INotificationService
Data Layer           →  DigitalFormsSystemContext (EF Core), DbInitializer,
                         Models (Employee, FixedAssetRequest, DamagedReport, AssetType)
Database Layer       →  SQL Server (Employees, FixedAssetRequests, DamagedReports,
                         AssetTypes, Approvals, StatusHistory, PrintLogs)
```

Each layer only depends on the layer directly below it (Controllers call Services, Services call the DbContext), which keeps business logic out of the controllers and makes the data access layer swappable/testable.

## 📁 Project Structure

```
DigitalFormsSystem/
├── DigitalFormsSystem/           # MVC Web project (Controllers, Views)
├── DigitalFormsSystem.Core/      # Shared class library (Models, Interfaces, Services, DbContext)
├── scripts/                      # Deployment automation scripts
│   ├── redeploy.ps1              # Stop app pool → publish → start app pool
│   └── redeploy.bat              # Double-click wrapper for redeploy.ps1
└── DigitalFormsSystem.sln
```

---

## 🚀 Local Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server / SQL Server Express
- IIS with [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (for IIS deployment; optional for local `dotnet run`)

### Configuration

1. Clone the repo:
   ```bash
   git clone https://github.com/CjConvento/DigitalFormsSystem.git
   ```
2. Update the connection string in `appsettings.json` to match your local SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=DigitalFormsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
3. Apply EF Core migrations:
   ```bash
   dotnet ef database update
   ```
4. Run locally:
   ```bash
   dotnet run --project DigitalFormsSystem
   ```

### Deploying to IIS

1. Ensure IIS + ASP.NET Core Hosting Bundle are installed.
2. Create an Application Pool with **.NET CLR Version: No Managed Code**.
3. Publish and deploy using the included script:
   ```powershell
   cd scripts
   .\redeploy.ps1
   ```
   Or double-click `redeploy.bat` (Run as Administrator).
4. Grant the App Pool identity access to the SQL Server database (`IIS APPPOOL\<YourAppPoolName>`).

---

## 🔒 Security Notes

- Passwords are hashed using BCrypt before storage.
- Session-based authentication with role checks on protected actions.
- ⚠️ Known improvement in progress: default/temporary password generation is being migrated from a predictable pattern to cryptographically random values with hashed storage.

---

## 📌 Roadmap

- [ ] Replace predictable temp password generation with random + hashed values
- [ ] Complete Shared Class Library + Blazor Server refactor
- [ ] Additional form modules

---

## 👤 Author

**Cyrenz Jonathan O. Convento**
BSIT Graduate — San Sebastian College, Recoletos de Cavite
Developed during internship at HS Technologies (Phils.), Inc., continued as a personal portfolio project.