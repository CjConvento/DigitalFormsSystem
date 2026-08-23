# Digital Forms System

**Status: Active development** — core modules and auth are functional; search/filter and a few other features are still in progress (see [Roadmap](#-roadmap)).

A fullstack internal web application built for **HS Technologies (Phils.), Inc. (HST)**, developed during a 600-hour internship in the MIS-IT section. The system digitizes two core company processes — **Fixed Asset Requests** and **Damaged Reports** — replacing manual paper-based workflows with a role-based, database-driven web application.

> ⚠️ This repository is a personal portfolio continuation of an internship deliverable. It does not contain any real company data. All employee records, credentials, and department data used for local development/testing are synthetic/dummy data.

---

## 📋 Business Context

HST previously ran these two processes on paper forms (`HRDA-F-2.10.6.1` for Fixed Asset Requests, `HRDA-F-2.9.10.8` for Damaged Reports), routed manually between departments for approval and filing.

| | Paper-based | This system |
|---|---|---|
| Submission | Physical form, hand-carried to GAD-HR | Web form, submitted online |
| Tracking | Status written on the same sheet | Status history stored in the database |
| Visibility | Whoever has the physical copy | Role-based — Employee sees own records, IT Manager sees all |
| Records | Filed in cabinets | Queryable in SQL Server |

The system replicates the same approval fields and workflow from the original forms, just digitized.

---

## ✨ Features

- **Role-based access control (RBAC)** — separate views and permissions for IT Manager and Employee roles
- **Session-based authentication** with BCrypt password hashing
- **Secure random password generation** for employee onboarding (`EmployeeNo@Random`)
- **Fixed Asset Request module** — submit, track, and manage asset requests
- **Damaged Report module** — report and manage damaged equipment/tools
- **Audit logging** — tracks all user actions (login, create, edit, delete)
- **Admin Dashboard** — with statistics and audit log viewer

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | C# / ASP.NET Core MVC (.NET 8.0) |
| Database | SQL Server (Entity Framework Core 8.0.0) |
| Auth | Session-based auth, BCrypt.Net-Next for password hashing |
| Password Generation | Cryptographically secure random (`RandomNumberGenerator`) |
| PDF Generation | Python script (external, admin-run only) |
| Hosting | IIS (ASP.NET Core Hosting Bundle + In-Process hosting) |
| Version Control | Git / GitHub |

---

## 🏗️ Architecture

The system follows a layered architecture:

```
Presentation Layer   →  Razor Views, CSS/JS
Controller Layer     →  Home, Account, FixedAsset, Damaged, Audit
Service Layer        →  ICurrentUserService, IFixedAssetRequestService,
                         IDamagedReportService, INotificationService, IAuditService
Data Layer           →  DigitalFormsSystemContext (EF Core), DbInitializer,
                         Models (Employee, FixedAssetRequest, DamagedReport, AuditLog, etc.)
Database Layer       →  SQL Server (Employees, FixedAssetRequests, DamagedReports,
                         AuditLogs, AssetTypes, Approvals, StatusHistory, PrintLogs)
```

Each layer only depends on the layer directly below it, keeping business logic out of controllers and making the data access layer swappable/testable.

---

## 📁 Project Structure

```
DigitalFormsSystem/
├── DigitalFormsSystem/                        # MVC Web project (Controllers, Views, Program.cs)
├── DigitalFormsSystem.Core/                   # Shared class library (Models, Interfaces, Services, Helpers, DbContext)
├── Data/                                      # DbInitializer (password seeding)
├── Helpers/                                   # PasswordGenerator (random password generation)
├── scripts/                                   # Deployment automation scripts
│   ├── redeploy.ps1                           # Stop app pool → publish → start app pool
│   └── redeploy.bat                           # Double-click wrapper for redeploy.ps1
├── generate_employee_passwords_from_db.py     # Python script for manual PDF export (admin only)
├── appsettings.template.json                  # Configuration template (copy to appsettings.json)
├── Directory.Build.props                      # Shared MSBuild properties
└── DigitalFormsSystem.sln
```

---

## 🔐 Security Features

- **BCrypt password hashing** — passwords are never stored in plain text
- **Cryptographically secure random password generation** — `EmployeeNo@Random` format (e.g., `HS9501@XK9#mP2`)
- **Role-based access control** — only the IT Manager can see all requests/reports and audit logs
- **Audit logging** — all login attempts, CRUD operations, and sensitive actions are logged
- **Session-based authentication** with 30-minute timeout
- **No sensitive data exposed** — `appsettings.json` is excluded from version control
- **No in-app credential export** — the employee credentials PDF is generated by a separate, admin-run script (see [Manual Credential Export](#-manual-credential-export)), not by a button in the web UI

---

## 🚀 Local Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server / SQL Server Express
- IIS with [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (for IIS deployment; optional for local `dotnet run`)
- Python 3.x (for manual PDF generation)

### Configuration

1. Clone the repo:
   ```bash
   git clone https://github.com/CjConvento/DigitalFormsSystem.git
   cd DigitalFormsSystem/DigitalFormsSystem
   ```

2. Copy `appsettings.template.json` to `appsettings.json`:
   ```bash
   cp appsettings.template.json appsettings.json
   ```

3. Update the connection string in `appsettings.json` to match your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=DigitalFormsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

4. Apply EF Core migrations:
   ```bash
   dotnet ef database update --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
   ```

5. Run locally:
   ```bash
   dotnet run
   ```

6. Access the application at `http://localhost:5280`.

### Default Login

| Role | Employee No. | Password |
|---|---|---|
| Admin | HS1005-1301 | Randomly generated (check database or use the Python script) |
| Employee | HS9501-0019 | Randomly generated |

---

## 📄 Manual Credential Export

The system does **not** include a PDF download button in the web UI, by design. Instead, the IT Manager can generate the employee credentials PDF locally using a separate script:

```bash
python generate_employee_passwords_from_db.py
```

This keeps credential export out of the web attack surface entirely — it requires direct database and machine access, not just an authenticated web session.

---

## 🚀 Complete Deployment Guide (for HST server)

### Prerequisites (on the server)

- Windows Server with IIS installed
- [.NET 8.0 Runtime and ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) — install both `dotnet-hosting-8.0.x-win.exe` and `dotnet-runtime-8.0.x-win.exe`
- SQL Server (any edition), with a `DigitalFormsSystem` database created
- Windows Admin rights (for IIS configuration)

### Step 1: Deploy the Code

**Option A — via Git (recommended):**
```powershell
cd C:\inetpub\wwwroot
git clone https://github.com/CjConvento/DigitalFormsSystem.git
cd DigitalFormsSystem/DigitalFormsSystem
```

**Option B — manual copy:**
```powershell
# From your dev machine
dotnet publish -c Release -o C:\Publish\DigitalFormsSystem

# Then copy the contents of C:\Publish\DigitalFormsSystem to the server, into:
# C:\inetpub\wwwroot\DigitalFormsSystem
```

### Step 2: Configure appsettings.json

On the server, create `appsettings.json` from `appsettings.template.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=DigitalFormsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AppSettings": {
    "ManagerEmployeeId": 778,
    "PasswordMasterKey": "YOUR_MASTER_KEY_HERE"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

| Placeholder | Replace With |
|---|---|
| `YOUR_SERVER_NAME` | SQL Server name (e.g., `SRV-SQL-01` or `192.168.1.100`) |
| `YOUR_MASTER_KEY_HERE` | A secure random key (min. 16 characters) — generate fresh, don't reuse the dev value |

> ⚠️ `appsettings.json` is excluded from version control. Never commit the real connection string or master key.

### Step 3: Create IIS Application Pool

1. Open IIS Manager
2. Right-click **Application Pools** → **Add Application Pool**
3. Name: `DigitalFormsSystemPool`
4. **.NET CLR Version: No Managed Code**
5. Managed Pipeline Mode: Integrated
6. Start Application Pool Immediately: checked
7. OK

### Step 4: Create IIS Website

1. Right-click **Sites** → **Add Website**
2. Site name: `DigitalFormsSystem`
3. Application pool: `DigitalFormsSystemPool`
4. Physical path: `C:\inetpub\wwwroot\DigitalFormsSystem\DigitalFormsSystem`
5. Binding: Type `http`, IP `All Unassigned`, Port `80` (or `8080` for internal-only access), Host name blank unless using a domain
6. OK

### Step 5: Set Folder Permissions

```powershell
icacls C:\inetpub\wwwroot\DigitalFormsSystem\DigitalFormsSystem /grant "IIS AppPool\DigitalFormsSystemPool:(OI)(CI)M"
```

### Step 6: Run Database Migrations

```powershell
cd C:\inetpub\wwwroot\DigitalFormsSystem\DigitalFormsSystem
dotnet ef database update --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
```

### Step 7: Seed Employee Data

```powershell
dotnet run
# Expected output:
# ⏳ Seeding passwords for 683 employees...
# ✅ Password seeding completed! 683 employees updated.
```

### Step 8: Generate Initial Passwords for Gilbert

```powershell
python generate_employee_passwords_from_db.py
# Output: employee_credentials_from_db.pdf
```

Hand this PDF to **Gilbert (IT Manager)** directly, via a secure channel (in person or encrypted transfer) — not email or chat. It contains every employee's initial password and should never sit in a shared folder or be forwarded further than necessary.

### Step 9: Configure Windows Firewall

```powershell
New-NetFirewallRule -DisplayName "DigitalFormsSystem-HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
```

### Step 10: Test the Application

- On the server: `http://localhost/Account/Login`
- From another machine on the network: `http://[SERVER_IP]/Account/Login`

### Step 11: (Optional) Enable HTTPS

1. Install an SSL certificate in IIS
2. Bind HTTPS to port 443
3. Enable **Require SSL** in IIS

### Step 12: Redeploying After Code Changes

Use the existing `scripts/redeploy.ps1` (see [Project Structure](#-project-structure)) — it already handles stop → publish → start:

```powershell
cd scripts
.\redeploy.ps1
```

---

## 📋 Quick Reference Card (for HST IT)

| Item | Value |
|---|---|
| App Pool Name | `DigitalFormsSystemPool` |
| .NET CLR Version | No Managed Code |
| Physical Path | `C:\inetpub\wwwroot\DigitalFormsSystem\DigitalFormsSystem` |
| Port | 80 (HTTP) / 443 (HTTPS) |
| Database | SQL Server: `DigitalFormsSystem` |
| Admin Login | `HS1005-1301` (Gilbert) |

### Support

| Issue | Solution |
|---|---|
| 500.19 Error | Check `web.config` and confirm the .NET Core Hosting Bundle is installed |
| 502.5 Error | Check Event Viewer → Application logs |
| Database connection fails | Verify the connection string in `appsettings.json` |
| Permissions error | Grant `IIS AppPool\DigitalFormsSystemPool` access to the folder |

### Troubleshooting

**500.19** — install the [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

**"Cannot find dotnet"**
```powershell
setx PATH "%PATH%;C:\Program Files\dotnet"
```

**Database connection failed** — grant the app pool identity access:
```sql
CREATE LOGIN [IIS APPPOOL\DigitalFormsSystemPool] FROM WINDOWS;
ALTER LOGIN [IIS APPPOOL\DigitalFormsSystemPool] ENABLE;
EXEC sp_addrolemember 'db_datareader', 'IIS APPPOOL\DigitalFormsSystemPool';
EXEC sp_addrolemember 'db_datawriter', 'IIS APPPOOL\DigitalFormsSystemPool';
```

### Deployment Checklist

- [ ] .NET 8.0 Hosting Bundle installed
- [ ] SQL Server installed and running
- [ ] Database `DigitalFormsSystem` created
- [ ] `appsettings.json` configured
- [ ] IIS Application Pool created
- [ ] IIS Website created
- [ ] Folder permissions set
- [ ] Database migrations run
- [ ] Employee data seeded
- [ ] PDF generated and handed to Gilbert securely
- [ ] Firewall rule added
- [ ] HTTPS configured (if needed)
- [ ] Tested from another machine on the network

---

## 🔄 Recent Updates

- **Password generation** — migrated from a predictable `HST{EmployeeNo}!` pattern to cryptographically random `EmployeeNo@Random` values, hashed with BCrypt before storage
- **Credential export** — moved out of the web app entirely, into an external, admin-run Python script (no in-app download endpoint)
- **Audit logging** — fully implemented with an admin-only view
- **Build fixes** — resolved duplicate assembly attribute errors via `Directory.Build.props`
- **Security hardening** — `appsettings.json` removed from version control

---

## 📌 Roadmap

- [ ] Add session timeout configuration
- [ ] Add login attempt lockout
- [ ] Add search and filter for requests/reports
- [ ] Complete Blazor Server refactor

---

## 👤 Author

**Cyrenz Jonathan O. Convento**
BSIT Graduate — San Sebastian College, Recoletos de Cavite
Developed during internship at HS Technologies (Phils.), Inc., continued as a personal portfolio project.

## 📄 License

This project is for portfolio and educational purposes only.