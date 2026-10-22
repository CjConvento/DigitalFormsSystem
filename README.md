# DFS: Fixed Assets & Damaged Reports Internal Web Application

[![Status: Ready for Production](https://shields.io)](#-roadmap)
[![Target Deployment](https://shields.io)](#on-premise-enterprise-deployment-guide-for-hst-server)
[![Database](https://shields.io)](#technology-stack)

> **Ready for Deployment:** This guide documents the steps for HST's IT team to deploy the system to their on-premise infrastructure. The system has been fully tested on staging environment and is production-ready. Advanced search filters and multi-stage workflow expansions are currently in progress (see [Roadmap](#-roadmap)).

A monolith internal enterprise web application engineered for **HS Technologies (Phils.), Inc. (HST)**, developed during a 600-hour industry internship within the MIS-IT Department. The system digitizes critical corporate procedures—**Fixed Asset Requests** and **Damaged Reports**—replacing manual, paper-based operations with a secure, role-based, database-driven solution designed for **600+ internal employees**.

> **Security & Data Privacy Notice:** This public repository contains **zero real employee data or corporate records**. All data utilized during development and testing is entirely synthetic. Production deployment, including real employee synchronization, is managed strictly on-premise by HST's IT Infrastructure team within their private network.

---

## 📌 Table of Contents
- [Business Architecture Context](#business-architecture-context)
- [System Features](#system-features)
- [Technology Stack](#technology-stack)
- [Architecture Design](#architecture-design)
- [Project Directory Tree](#project-directory-tree)
- [Local Developer Staging Setup](#local-developer-staging-setup)
- [On-Premise Enterprise Deployment Guide (For HST Server)](#on-premise-enterprise-deployment-guide-for-hst-server)
- [Networking & Firewall Instruction](#networking--firewall-instruction)
- [Diagnostics & Troubleshooting Support](#diagnostics--support)
- [Recent Architectural Updates](#recent-architectural-updates)
- [Roadmap](#-roadmap)
- [Project Author](#project-author)

---

## Business Architecture Context

HST previously handled internal processes via physical paper routing forms routed to the MIS-IT Dept. for approvals, which have now been transitioned to an instant digital web form submission system with automated tracking, role-based access control, secure archiving, and file attachment persistence.

The digitized application accurately replicates the business logib of the original forms.

| Operation Feature | Legacy Paper-Based Workflow | Digitized Enterprise System |
| :--- | :--- | :--- |
| **Submission** | Physical form routing, hand-carried to IT Manager | Instant digital web form submission |
| **Tracking** | Manual pen-and-paper signature tracking | Automated real-time state logs in SQL Server |
| **Data Visibility** | Restricted to physical document possessor | Role-Based Access (Employees view own; IT Admin views all) |
| **Record Archiving** | Physical file cabinets, manual auditing | Secure, fully queryable indexing via Relational Database |
| **File Attachment** | Stapled physical photos / documents | Automated local volume persistence with encrypted directory mapping |

The digitized application accurately replicates the business logic, approval chains, and structural fields of the original enterprise forms.

---

## System Features

* **Role-Based Access Control (RBAC):** Strict isolation of user views, operations, and dashboard access based on authorization tiers (Administrator vs. Employee).
* **Session-Based Authentication:** Secure user management utilizing encrypted cookie sessions with automatic 30-minute idle timeouts.
* **Cryptographic Security:** Secure password implementation powered by the BCrypt hashing algorithm (`BCrypt.Net-Next`) for zero plain-text storage.
* **Atomic Transactions:** Multi-step operations (e.g., Main Request + Nested Details + File Uploads) are wrapped in database transactions to guarantee data consistency and prevent orphaned records.
* **Audit Logging System:** Comprehensive internal tracking that records every system transaction, data modification, and login event (including IP and User-Agent) for enterprise compliance.
* **Administrative Dashboard:** Centralized console rendering operational metrics, system health logs, and audit trails.

---

## Technology Stack

| Architecture Layer | Component / Technology |
| :--- | :--- |
| **Frontend UI** | ASP.NET Core Razor Views |
| **Backend Core** | C# / ASP.NET Core (.NET 8.0) |
| **Data / ORM** | **Microsoft SQL Server** via Entity Framework Core (EF Core 8.0) |
| **File Storage** | **Local Storage** (Self-hosted) |
| **Security / Auth** | BCrypt Password Hashing, Session-based Auth, Global Exception Middleware |
| **Architecture Patterns** | Thin Controllers, DTOs, Static Mappers, Atomic Transactions, Service Layer |
| **Target Deployment** | On-Premise Internet Information Services (IIS) Server |
| **Automation Utilities** | Python 3.x (Admin script for discrete, local cryptography management) |

---

## Architecture Design

The system follows a strict, decoupled Layered System Architecture to ensure maintainability, clear separation of concerns, and clean testing boundaries:

```text
Presentation Layer   →  Razor Views, CSS/JS, Global Exception Middleware
Controller Layer     →  Thin Controllers (HTTP Routing, Session Management, DTO Mapping only)
Mapping Layer        →  Static Extension Mappers (Clean Entity ↔ DTO transformations)
Service Layer        →  Core Services (Account, Audit, DamagedReport, FixedAssetRequest, Notification, AppwriteStorage)
Data Transfer Layer  →  DTOs (Decoupled from EF Core Entities to prevent over-posting)
Data Layer           →  DigitalFormsSystemContext (EF Core), DbInitializer
Database Layer       →  SQL Server (Employees, FixedAssetRequests, DamagedReports, AuditLogs, etc.)
Storage Layer        →  Local File Storage (File Server)
```

---

## Project Directory Tree

```text
DigitalFormsSystem/                              # Source Folder
├── DigitalFormsSystem/                          # MVC Web project (Controllers, Views, Program.cs, Middleware)
├── DigitalFormsSystem.Core/                     # Core Business Logic Library (Single Source of Truth)
│   ├── Dtos/                                    # Data Transfer Objects
│   ├── Interfaces/                              # Service Contracts
│   ├── Mappers/                                 # Static Extension Methods for Entity ↔ DTO mapping
│   ├── Models/                                  # EF Core Entities, DbContext and Migration configurations
│   └── Services/                                # Business Logic (Account, Audit, DamagedReport, FixedAsset, etc.)
├── Data/
    └── DbInitiliazer.cs                         # In-memory password seeding for employees, used for manual PDF employees details and passwords export (admin only)
├── scripts/                                     # Deployment automation scripts
│   └── redeploy.ps1                             # Stop app pool → publish → start app pool
├── appsettings.template.json                    # Configuration template (copy to appsettings.json)
└── DigitalFormsSystem.sln                       # Project Solution
```

---

## Local Developer Staging Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) [10]
- Microsoft SQL Server (LocalDB or Express Edition) [12]
- Internet Information Services (IIS) with [.NET 8.0 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) *(Optional, required only for local IIS testing)* [10, 12]

### Configuration Workflows

1. Open your terminal or command prompt and clone the repository:
   ```bash
   git clone https://github.com/CjConvento/DigitalFormsSystem.git
   cd DigitalFormsSystem
   ``` [12]

2. Establish local development environment settings profiles:
   ```bash
   cp appsettings.template.json appsettings.json
   ``` [12]

3. Update the targeted Context Database String inside your local `appsettings.json` directory:
   ```json
   "ConnectionStrings": {
     "DigitalFormsSystemContext": "Server=localhost\\SQLEXPRESS;Database=DigitalFormsSystem;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
   }
   ```

4. Execute Entity Framework Core Relational Migrations to build out local tables:
   ```bash
   dotnet ef database update --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
   ``` [12]

5. Launch the development server application:
   ```bash
   dotnet run
   ``` [12]

6. Access the live interface layer inside your web browser at: `http://localhost:{SERVER PORT}`.

---

## 🔒 In-Memory Transient Credential Provisioning

By engineering design, this enterprise web application contains **zero public endpoints or download parameters** to export employee access records within the user UI context layout layer, eliminating bulk harvesting vectors entirely.

Instead, the credential synchronization workflow occurs strictly during the system's database seeding lifecycle:
* **The Workflow:** When the application transitions into an active running state, the internal `DbInitializer` pipeline dynamically intercepts unseeded employee rows inside runtime memory (RAM).
* **Cryptographic Layer:** The mechanism assigns temporary activation tokens, computes secure `BCrypt` hashes for database storage, and directly exports an encrypted onboarding file named `employee_credentials_report.pdf` into the root folder using native C# compiling loops.
* **Security Enforcement:** This physical document file is meant to be printed immediately by the network administrator for secure distribution, after which the local digital asset can be completely deleted from the environment volume.

---

## On-Premise Enterprise Deployment Guide (for HST server)

### Server Infrastructure Prerequisites
- Windows Server running active Internet Information Services (IIS).
- [.NET 8.0 Runtime and ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
- Dedicated **Microsoft SQL Server** instance with an empty initialized `DigitalFormsSystem` database target catalog.
- Local filesystem access with directory authorization privileges for native host folder storage.
- Windows Local Administrator security access privileges.

### Step-by-Step Deployment Lifecycle

#### Step 1: Clone or Download the Source Code
Select the most appropriate acquisition pathway based on the software utilities available on the deployment host machine:

<details>
<summary><b>Option A: Native Terminal Interface (CMD / PowerShell) - Recommended</b></summary>

1. Open PowerShell or Command Prompt as an Administrator.
2. Execute the `git clone` command interface string to pull the repository branch:
   ```bash
   git clone https://github.com/CjConvento/DigitalFormsSystem
   cd DigitalFormsSystem
   ```
</details>

<details>
<summary><b>Option B: Visual Studio IDE Graphical Interface</b></summary>

1. Launch **Visual Studio** on the application deployment server.
2. On the startup dialog panel, select **Clone a repository**.
3. Paste the repository URL destination string: `https://github.com/CjConvento/DigitalFormsSystem`
4. Assign your local repository path parameters and click **Clone**.
5. Once loaded, open the **Terminal** window within Visual Studio (`Ctrl + ` ` `) to prepare for compilation commands.
</details>

<details>
<summary><b>Option C: Visual Studio Code Editor (VS Code)</b></summary>

1. Launch **VS Code** on the host operating system instance.
2. Open the Command Palette (`Ctrl + Shift + P`) and execute: `Git: Clone`.
3. Input the destination repository URL link: `https://github.com/CjConvento/DigitalFormsSystem`
4. Select the target directory location parameters and click **Open**.
5. Open an integrated terminal instance directly inside VS Code (`Ctrl + Shift + ` ` `).
</details>

<details>
<summary><b>Option D: Offline Manual Transition (ZIP Archive Method)</b></summary>

1. Access the repository URL link on any local web browser utility application.
2. Click the green **Code** actions trigger item on the top-right console panel area.
3. Select **Download ZIP** from the interactive drop-down layout profile.
4. Copy the compiled `.zip` file over to the host machine volume via secure network directory transport channels.
5. Extract the root deployment files directly into your destination staging workspace folder.
</details>

---

#### Step 2: Deploy Compilation Artifacts

1. Open your IDE and clone the repository:
   ```bash
   git clone https://github.com/CjConvento/DigitalFormsSystem
   cd DigitalFormsSystem
   ```
2. Navigate to the main web project directory:
   ```bash
   cd DigitalFormsSystem
3. Build production-ready release artifacts locally or run the compiler on the machine:
```powershell
dotnet publish -c Release -o C:\Publish\DigitalFormsSystem
```

#### Step 3: Establish Corporate Production Configuration
Generate a secure `appsettings.json` file inside the host destination folder:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=DigitalFormsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AppSettings": {
    "ManagerEmployeeId": 778,
    "PasswordMasterKey": "YOURMASTERKEY",
    "MaxFileSizeMB": 100
  },
    "StorageSettings": {
    "Provider": "LocalIIS",
    "ConnectionString": ""
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

> ⚠️ **CRITICAL SECURITY NOTE:** `appsettings.json` contains production environment keys and is systematically excluded from version control tracking rules. Never commit connection parameters to public codebases.

#### Step 4: Configure Dedicated IIS Application Pool
1. Open the **IIS Manager** control interface.
2. Right-click **Application Pools** ➔ Select **Add Application Pool**.
3. Apply structural profile adjustments:
   * **Name:** `DFSApp`
   * **.NET CLR Version:** `No Managed Code`
   * **Managed Pipeline Mode:** `Integrated`
4. Confirm **"Start application pool immediately"** is checked.

#### Step 5: Provision Web Site Architecture
1. Right-click **Sites** ➔ Select **Add Website**.
2. Assign architectural parameters:
   * **Site name:** `DigitalFormsSystem`
   * **Application pool:** `DFSApp`
   * **Physical path:** `C:\inetpub\wwwroot\DigitalFormsSystem` or specify your path
   * **Binding:** Type `http`, Port `80` *(or specify `8080` for restricted internal intranet staging networks)*.

#### Step 6: Establish Process Permissions
Grant the IIS Application Pool Identity appropriate access permissions:
```powershell
icacls C:\inetpub\wwwroot\DFSApp /grant "IIS AppPool\DFSApp:(OI)(CI)M"
icacls C:\inetpub\wwwroot\DFSApp\wwwroot\uploads /grant "IIS AppPool\DFSApp:(OI)(CI)M"
```

### Step 6.5: Grant Database Permissions to IIS App Pool
Because Windows Authentication is what the applicaion uses (`Trusted_Connection=True`), we need to give access to IIS App Pool identity on SQL Server. Execute this script in **SQL Server Management Studio (SSMS)** using an admin account:
```sql
-- 1. Create Login for IIS App Pool Identity
USE [master];
GO
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'IIS AppPool\DFSApp')
BEGIN
    CREATE LOGIN [IIS AppPool\DFSApp] FROM WINDOWS;
END
GO

-- 1.5 Set Default Database (Crucial Step to prevent login failures)
ALTER LOGIN [IIS AppPool\DFSApp] WITH DEFAULT_DATABASE = [DigitalFormsSystem];
GO

-- 2. Grant access to the DigitalFormsSystem database
USE [DigitalFormsSystem];
GO
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'DFSAppUser')
BEGIN
    CREATE USER [DFSAppUser] FOR LOGIN [IIS AppPool\DFSApp];
END
GO

-- 3. Grant db_owner role (required for EF Core migrations and full data operations)
ALTER ROLE db_owner ADD MEMBER [DFSAppUser];
GO

PRINT '✅ IIS App Pool permissions and default database successfully configured!';
```

### Step 7: Database Schema Initialization
This step will only create the new DFS-specific tables (e.g., `DamagedReports`, `FixedAssetRequests`, `AuditLogs`). Choose one of the following methods based on your IT policy:

#### Option A: Manual via Raw SQL Script (For strict DBA environments)
If running `dotnet ef` on the production environment is restricted, execute the schema script in SSMS:
1. Open **SSMS** and connect as an Administrator.
2. Copy the entire contents of `scripts/init_schema.sql` from this repository.
3. Execute the script. It uses `IF NOT EXISTS` guards to safely create only the missing Database,DFS tables, indexes, and foreign keys.

#### Option B: Automated via EF Core Migrations (Via Source Terminal)
If the .NET SDK is available directly on the server, execute this in PowerShell:
```powershell
cd C:\inetpub\wwwroot\DFSApp
dotnet ef database update --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
```

## Step 7.5: Migrate Existing Employee Data to DigitalFormsSystem

Before running the application for password seeding, existing employee records must be migrated from the current MIS IT Employees database to the new `DigitalFormsSystem` database. To ensure authorization permissions persist correctly, the original primary key IDs must be explicitly preserved.

### Option A: Automated via Cross-Database SQL Script (Recommended & Fastest)

If the source Employees database and the `DigitalFormsSystem` database are on the same SQL Server instance, execute this script in SSMS to force-insert the original IDs safely:

```sql
USE [DigitalFormsSystem];
GO

-- 👑 CRITICAL: Enable identity insertion to preserve original IDs
SET IDENTITY_INSERT dbo.Employees ON;
GO

INSERT INTO dbo.Employees (
    ID, EmployeeNo, Name, DateHired, Company, Location, Department, Section, Category, Status, IsActive, CreatedAt
)
SELECT
    ID, EmployeeNo, Name, DateHired, Company, Location, Department, Section, Category, Status, IsActive, CreatedAt
FROM [Source_Employees_DB].dbo.Employees
WHERE IsActive = 1; -- Import only active employees

-- 👑 CRITICAL: Turn identity insertion back off after completion
SET IDENTITY_INSERT dbo.Employees OFF;
GO

SELECT COUNT(*) AS ImportedEmployees FROM dbo.Employees;
PRINT '✅ Employee data successfully migrated with original IDs preserved.';
```

### Option B: Manual via SSMS Import/Export Wizard (GUI Approach)

1. In SSMS Object Explorer, right-click the `DigitalFormsSystem` database ➔ **Tasks** ➔ **Import Data...**
2. **Choose a Data Source:** Select *SQL Server Native Client* and point to the source Employees database.
3. **Choose a Destination:** Point to the `DigitalFormsSystem` database.
4. **Select Source Tables:** Check the source `Employees` table.
5. **CRITICAL MAPPING & COMPLIANCE STEP:** Click **Edit Mappings**.
   * **Map the `ID` Column:** Ensure the source `ID` is mapped directly to the destination `ID`.
   * **Enable Identity Insert:** Check the box that says **"Enable identity insert"**. *(This is mandatory so SQL Server allows the wizard to preserve original IDs instead of generating new ones).*
   * **Leave Security Columns Unmapped:** Ensure the destination-specific security fields (**`PasswordHash`** and **`IsFirstLogin`**) are set to **`<ignore>`**. Leaving them unmapped allows them to safely remain `NULL` for the in-memory C# seeding process in [Step 8: Initial Login and System Health Check](#step-8-initial-login-and-system-health-check).
6. Click **Next** and **Finish** to start the migration.

---

### Step 8: Start and Verify the Application
Since the application is hosted on IIS, it runs automatically once the App Pool is started.

1. Open your web browser and navigate to the configured URL (e.g., `http://localhost`, `http://hst-dfs.local`, or the server's IP address like `http://192.168.x.x:8070`).
2. **First Load Note (Cold Start):** The very first time you access the site after deployment, it may take 3 to 5 seconds to load. This is normal .NET JIT compilation and EF Core initialization behavior. Subsequent loads will be instant (under 1 second).
3. **Automatic In-Memory Password Seeding:** Upon this first successful connection, the application's built-in `DbInitializer` will intercept active employee accounts. It will compute secure cryptographic BCrypt hashes to save in the database, while simultaneously generating a one-time onboarding report named `employee_credentials_report.pdf` directly inside the authorized storage folder (`C:\inetpub\wwwroot\DFSApp\wwwroot\uploads`).
4. **Security Enforcement:** Print the physical copies of the generated PDF report immediately for secure distribution, then permanently delete the digital PDF file from the server volume and empty the Windows Recycle Bin.

### Step 9: Initial Login and System Health Check
1. Open the login page and authenticate using any active **Employee Number** from the system.
   * **Password:** Use the unique temporary setup token allocated to that user, which you retrieved from the printed hardcopy of the onboarding report generated during Step 7.
2. Once logged in, verify the following:
   * **Dashboard:** Check if metrics and audit logs are rendering and displaying correctly.
   * **File Uploads:** Try creating a test Damaged Report or Fixed Asset Request and upload an image. Verify that the file is successfully saved to the folder `C:\inetpub\wwwroot\DFSApp\wwwroot\uploads` and displays correctly in the UI.
   * **Permissions:** Ensure that regular employees can only see their own requests, while the Manager/Admin (Employee ID 778) can see all requests.

### Step 10: Future Updates and Automated Redeployment
When you need to deploy future updates or bug fixes, do not manually copy-paste files to avoid thread execution errors. Instead, use the automated redeployment pipeline scripts included in the repository to ensure clean application pool recycling:

1. Locate `scripts/redeploy.bat` and `scripts/redeploy.ps1` within this repository.
2. Open `scripts/redeploy.ps1` and verify or adjust the core parameters inside the configuration block to ensure they point to your target project folder structure:
   ```powershell
   # ============================================================================
   # ⚙️ CONFIGURATION SYSTEM - SPECIFY YOUR ENVIRONMENT PATHS HERE
   # ============================================================================
   $appPoolName = "DFSApp"                        # Specify target IIS App Pool token
   $projectPath = ".\DigitalFormsSystem.csproj"   # Specify native C# project source path
   $outputPath  = "C:\inetpub\wwwroot\DFSApp"     # Specify destination path on IIS host
   # ============================================================================
   ```
3. Copy both `redeploy.bat` and `redeploy.ps1` natively to the root of your deployment directory (`C:\inetpub\wwwroot\DFSApp`).
4. Right-click the administrative script wrapper `redeploy.bat` and select **Run as Administrator**.
5. The automation utility will safely stop the `DFSApp` application pool, run the `dotnet publish` command to build optimized production assets, swap files, and restart the service cleanly with zero manual intervention.

---

## Networking & Firewall Instruction
If the application is configured to run on a custom port (e.g., `8070`), the network administrator must explicitly add an **Inbound Rule** in the Windows Defender Firewall to allow traffic through this port. Without this rule, workstations across the corporate network will be blocked from accessing the system.

**Quick CLI Command (Run as Administrator in PowerShell):**
```powershell
New-NetFirewallRule -Name "DFSAppPort" -DisplayName "Digital Forms System (Port 8070)" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8070
```

---

## Diagnostics & Support
If you encounter any environmental exceptions, IIS permission flags, or SQL Server connection faults during the staging or production deployment lifecycles, please consult our dedicated documentation mapping guide:

👉 **[View the Full Troubleshooting & Diagnostics Guide (TROUBLESHOOTING.md)](./TROUBLESHOOTING.md)**

---

## Recent Architectural Updates
* **Architecture Refactor:** Migrated to a strict layered architecture. All business logic, DTOs, and Mappers are now consolidated in `DigitalFormsSystem.Core` (Single Source of Truth).
* **Thin Controllers:** Controllers now solely handle HTTP routing, session management, and DTO mapping, delegating all business logic to Services.
* **Atomic Transactions:** Implemented `await using var transaction` in core services to guarantee data consistency (e.g., Request + Nested Details + Storage) and prevent orphaned records.
* **Global Exception Handling:** Added centralized middleware to gracefully handle, log, and return standardized responses for unexpected errors.

---

## 📌 Roadmap
- [ ] Complete remaining approval stages (e.g., VP-level validation) for Fixed Asset Requests and Damaged Reports
- [ ] Incorporate additional workflow requirements as they're identified during rollout with HST
- [ ] Add advanced search and filter capabilities for requests/reports
- [ ] Implement login attempt lockout and session timeout warnings
- [ ] Complete Blazor Server refactor for specific administrative dashboards

---

## Project Author
**Natajimura**
- GitHub: [@CjConvento](https://github.com/CjConvento)
- LinkedIn: [Cy](https://www.linkedin.com/in/cyrenz-jonathan-convento-650a931b7/)
- Email: conventocj110@gmail.com
