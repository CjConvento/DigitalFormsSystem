# Digital Forms System

**Status: Active Development** — Core modules and authentication are fully functional; advanced search filters and multi-stage workflow expansions are currently in progress (see [Roadmap](#-roadmap)).

A full-stack internal enterprise web application engineered for **HS Technologies (Phils.), Inc. (HST)**, developed during a 600-hour industry internship within the MIS-IT Department. The system digitizes critical corporate procedures — **Fixed Asset Requests** and **Damaged Reports** — replacing manual, paper-based operations with a secure, role-based, database-driven solution designed for **683 internal employees**.

> **Security & Data Privacy Notice:** This public repository contains **zero real employee data or corporate records** — all data utilized during development and testing is entirely synthetic and generated via local scripting tools. Production deployment, including real employee synchronization, is managed strictly on-premise by HST's IT Infrastructure team within their private network.

---

## Business Architecture Context

HST previously handled these internal processes via standardized physical paper routing forms (`HRDA-F-2.10.6.1` and `HRDA-F-2.9.10.8`), hand-carried across departments for multi-stage approvals.

| Operation Feature | Legacy Paper-Based Workflow | Digitized Enterprise System |
|---|---|---|
| **Submission** | Physical form routing, hand-carried to GAD-HR | Instant digital web form submission |
| **Tracking** | Manual pen-and-paper signature tracking | Automated real-time state logs in SQL Server |
| **Data Visibility** | Restricted to physical document possessor | Role-Based Access (Employees view own; IT Admin views all) |
| **Record Archiving** | Physical file cabinets, manual auditing | Secure, fully queryable indexing via Relational Database |

The digitized application accurately replicates the business logic, approval chains, and structural fields of the original enterprise forms.

---

## System Features

- **Role-Based Access Control (RBAC):** Strict isolation of user views, operations, and dashboard access based on authorization tiers (Administrator vs. Employee).
- **Session-Based Authentication:** Secure user management utilizing encrypted cookie sessions with automatic 30-minute idle timeouts.
- **Cryptographic Security:** Secure password implementation powered by the BCrypt hashing algorithm (`BCrypt.Net-Next`) for zero plain-text storage.
- **Automated Onboarding Utilities:** Cryptographically secure random temporary password generation (`RandomNumberGenerator`) adhering to safe defaults (`EmployeeNo@Random`).
- **Audit Logging System:** Comprehensive internal tracking that records every system transaction, data modification, and login event for enterprise compliance.
- **Administrative Dashboard:** Centralized console rendering operational metrics, system health logs, and audit trails.

---

## Technology Stack

| Architecture Layer | Component / Technology |
|---|---|
| **Frontend UI** | ASP.NET Core Razor Views |
| **Backend Core** | C# / ASP.NET Core (.NET 8.0) |
| **Data / ORM** | SQL Server via Entity Framework Core (EF Core 8.0) |
| **Security / Auth** | BCrypt Password Hashing & Cryptographically Secure Pseudo-Random Number Generators (CSPRNG) |
| **Automation Utilities** | Python 3.x (Admin script for discrete, local cryptography management) |
| **Target Deployment** | On-Premise Internet Information Services (IIS) Server (In-Process Hosting Configuration) |
| **Version Control** | Git / GitHub Workflows |

---

## Architecture Design

The system follows a decoupled Layered System Architecture to ensure maintainability, clear separation of concerns, and clean testing boundaries:

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

---

## Project Directory Tree

```
DigitalFormsSystem/
├── DigitalFormsSystem/                          # MVC Web project (Controllers, Views, Program.cs)
├── DigitalFormsSystem.Core/                     # Shared class library (Models, Interfaces, Services, Helpers, DbContext)
├── Data/                                        # DbInitializer (password seeding)
├── Helpers/                                     # PasswordGenerator (random password generation)
├── scripts/                                     # Deployment automation scripts
│   ├── redeploy.ps1                             # Stop app pool → publish → start app pool
│   └── redeploy.bat                             # Double-click wrapper for redeploy.ps1
|   └── generate_employee_passwords_from_db.py   # Python script for manual PDF passwords export (admin only)
├── appsettings.template.json                    # Configuration template (copy to appsettings.json)
├── Directory.Build.props                        # Shared MSBuild properties
└── DigitalFormsSystem.sln
```

---

## System Hardening & Security Implementations

- **Zero Plain-Text Retention:** All user passwords pass through a salted BCrypt work factor prior to storage.
- **Out-of-Band Document Generation:** The temporary credential export utility is intentionally decoupled from the web application layer. It can only be executed locally via an administrative script requiring direct environment access, entirely eliminating web-based bulk enumeration attacks.
- **Environment Isolation:** Crucial infrastructure metrics, connection credentials, and master keys are stored outside the code repository using localized `appsettings.json` overlays.
- **Administrative Transparency:** Full audit trail implementation tracking login failures, row edits, and systemic parameter shifts.

---

## Local Developer Staging Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Microsoft SQL Server
- IIS with [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (for IIS deployment) 
- Python 3.x (For administrative CLI execution)

### Configuration Workflows

1. Clone the repository:
   ```bash
   git clone https://github.com/CjConvento/DigitalFormsSystem.git
   cd DigitalFormsSystem/DigitalFormsSystem
   ```

2. Establish local environment settings:
   ```bash
   cp appsettings.template.json appsettings.json
   ```

3. Update the Target Database String inside your local `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=DigitalFormsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

4. Execute Entity Framework Relational Migrations:
   ```bash
   dotnet ef database update --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
   ```

5. Run the app:
   ```bash
   dotnet run
   ```

6. Access the application at `http://localhost:{SERVER PORT}`.

---

## Manual Credential Export

The system does **not** include a PDF download button in the web UI, by design. Instead, the IT Manager can generate the employee credentials PDF locally using a separate script:

```bash
python generate_employee_passwords_from_db.py
```

This keeps credential export out of the web attack surface entirely — it requires direct database and machine access, not just an authenticated web session.

---

## On-Premise Enterprise Deployment Guide (for HST server)

### Server Infrastructure Prerequisites
- Windows Server running Active Internet Information Services (IIS)
- [.NET 8.0 Runtime and ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) — install both `dotnet-hosting-8.0.x-win.exe` and `dotnet-runtime-8.0.x-win.exe`
- SQL Server (any edition), with a `DigitalFormsSystem` database created
- Windows Administrator Rights (for IIS configuration)

### Step-by-Step Deployment Lifecycle

#### Step 1: Deploy Compilation Artifacts
Build production-ready release artifacts locally or run the compiler on the machine:
```powershell
dotnet publish -c Release -o C:\Publish\DigitalFormsSystem
# Deploy target output files safely into: C:\inetpub\wwwroot\DigitalFormsSystem
```

#### Step 2: Establish Corporate Production Configuration
Generate a secure `appsettings.json` file inside the host destination folder:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=INTERNAL_SQL_SERVER_IP;Database=DigitalFormsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AppSettings": {
    "ManagerEmployeeId": 778,
    "PasswordMasterKey": "COMPLEX_PRODUCTION_CRYPTOGRAPHIC_KEY"
  },
  "AllowedHosts": "*"
}
```

| Placeholder | Replace With |
|---|---|
| `YOUR_SERVER_NAME` | SQL Server name (e.g., `SRV-SQL-01` or `192.168.1.100`) |
| `YOUR_MASTER_KEY_HERE` | A secure random key (min. 16 characters) — generate fresh, don't reuse the dev value |

> ⚠️ `appsettings.json` is excluded from version control. Never commit the real connection string or master key.

#### Step 3: Configure Dedicated IIS Application Pool

1. Open IIS Manager
2. Right-click **Application Pools** → **Add Application Pool**
3. Name: `DigitalFormsSystemPool`
4. **.NET CLR Version: No Managed Code**
5. Managed Pipeline Mode: Integrated
6. Start Application Pool Immediately: checked
7. OK

#### Step 4: Provision Web Site Architecture

1. Right-click **Sites** → **Add Website**
2. Site name: `DigitalFormsSystem`
3. Application pool: `DigitalFormsSystemPool`
4. Physical path: `C:\inetpub\wwwroot\DigitalFormsSystem\DigitalFormsSystem`
5. Binding: Type `http`, IP `All Unassigned`, Port `80` (or `8080` for internal-only access), Host name blank unless using a domain
6. OK

#### Step 5: Establish Process Permissions
Grant the IIS Application Pool Identity appropriate access permissions to execute and run the runtime files safely:
```powershell
icacls C:\inetpub\wwwroot\DigitalFormsSystem /grant "IIS AppPool\DigitalFormsSystemPool:(OI)(CI)M"
```

### Step 6: Run Database Migrations

```powershell
cd C:\inetpub\wwwroot\DigitalFormsSystem\DigitalFormsSystem
dotnet ef database update --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
```

### Step 6.5: Import Employee Data

Employee data is **not included in this repo** and is not provided as a file by the developer. HST's IT Manager should populate the `Employees` table by extracting records directly from HST's existing employee database (e.g., via a Python or SQL script run on their own infrastructure), mapping fields to match the schema below.

**`Employees` table schema:**

```
[dbo].[Employees] (
  [ID],
  [EmployeeNo],
  [Name],
  [DateHired],
  [Company],
  [Location],
  [Department],
  [Section],
  [Category],
  [Status],
  [IsActive],
  [CreatedAt],
  [PasswordHash],
  [IsFirstLogin]
)
```

> ⚠️ **Before running the password seeding step below**, confirm `PasswordHash` is `NULL` or empty for all imported rows — otherwise the seeding logic may skip rows it thinks are already set up, and those employees will end up with no usable temp password. Clear the column first if needed:
> ```sql
> UPDATE [dbo].[Employees] SET [PasswordHash] = NULL;
> ```

### Step 7: Seed Employee Data

```powershell
dotnet run
# Expected output:
# ⏳ Seeding passwords for 683 employees...
# ✅ Password seeding completed! 683 employees updated.
```

### Step 8: Generate Initial Passwords for IT Admin & Employees

```powershell
python generate_employee_passwords_from_db.py
# Output: employee_credentials_from_db.pdf
```

This PDF is handed to IT Manager directly. It contains every employee's initial password and should never sit in a shared folder or be forwarded further than necessary.

> ⚠️ **This is a one-time setup step.** Once the PDF has been generated and confirmed by the IT Manager, the `PlainTextPassword` column should be **removed entirely** — not just cleared — since it has no further purpose after this initial seeding. In the source code:
> 1. Remove the `PlainTextPassword` property from `Employee.cs`
> 2. Remove any references to it in `DbInitializer.cs`
> 3. Generate and apply a migration to drop the column:
>    ```powershell
>    dotnet ef migrations add RemovePlainTextPasswordColumn --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
>    dotnet ef database update --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
>    ```
> After this, there is no remaining plaintext password anywhere in the system — only the BCrypt hash.

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

### Step 12: Redeploying After Code Changes (For Developer)

Use the existing `scripts/redeploy.ps1` (see [Project Structure](#-project-structure)) — it already handles stop → publish → start:

```powershell
cd scripts
.\redeploy.ps1
```

---

## Quick Reference Card (for HST IT)

| Item | Value |
|---|---|
| App Pool Name | `DigitalFormsSystemPool` |
| .NET CLR Version | No Managed Code |
| Physical Path | `C:\inetpub\wwwroot\DigitalFormsSystem\DigitalFormsSystem` |
| Port | 80 (HTTP) / 443 (HTTPS) |
| Database | SQL Server: `DigitalFormsSystem` |

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

## Recent Updates

- **Password generation** — migrated from a predictable `HST{EmployeeNo}!` pattern to cryptographically random `EmployeeNo@Random` values, hashed with BCrypt before storage
- **Credential export** — moved out of the web app entirely, into an external, admin-run Python script (no in-app download endpoint)
- **Audit logging** — fully implemented with an admin-only view
- **Build fixes** — resolved duplicate assembly attribute errors via `Directory.Build.props`
- **Security hardening** — `appsettings.json` removed from version control

---

## 📌 Roadmap

- [ ] Complete remaining approval stages (e.g., VP-level validation) for Fixed Asset Requests and Damaged Reports
- [ ] Incorporate additional workflow requirements as they're identified during rollout with HST
- [ ] Add session timeout configuration
- [ ] Add login attempt lockout
- [ ] Add search and filter for requests/reports
- [ ] Complete Blazor Server refactor

> The multi-stage approval workflow (based on HST's existing paper forms) is implemented for the core stages, but some steps — such as VP-level validation — are still in progress. Additional requirements are expected from HST's MIS-IT Manager as the system moves toward production use.

---

## Project Author

**Natajimura**
- GitHub: [@CjConvento](https://github.com/CjConvento)
- LinkedIn: [Cy](https://www.linkedin.com/in/cyrenz-jonathan-convento-650a931b7/)
- Email: conventocj110@gmail.com
