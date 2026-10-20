# DFS - Fixed Assets & Damaged Reports Internal Web Application — Staging Environment

> **Staging Branch:** This branch represents the pre-production staging environment. It is automatically deployed to **Render** and utilizes cloud-native managed services (**Supabase** for PostgreSQL and **Appwrite** for Object Storage).

**Status:** **Active Staging** — Used for integration testing, QA, and pre-production validation before merging to the `production` branch.

---

## 📌 Table of Contents
- [Staging Infrastructure & Cloud Providers](#️-staging-infrastructure--cloud-providers)
- [Architecture Overview](#️-architecture-overview)
- [Deployment Workflow (Render CI/CD)](#-deployment-workflow-render-cicd)
- [Environment Configuration (Render)](#-environment-configuration-render)
- [Local Development (Pointing to Staging)](#-local-development-pointing-to-staging)
- [Staging-Specific Limitations & Notes](#️-staging-specific-limitations--notes)
- [Roadmap (Staging Focus)](#-roadmap-staging-focus)
- [Project Author](#project-author)

---

## ☁️ Staging Infrastructure & Cloud Providers

Unlike the on-premise `production` branch, the `staging` environment is fully cloud-hosted to allow for rapid iteration and automated CI/CD.

| Component | Provider / Technology | Notes |
| :--- | :--- | :--- |
| **Hosting / PaaS** | **Render** | Auto-deploys on push to `staging`. Ephemeral filesystem. |
| **Relational Database** | **Supabase** (PostgreSQL) | Managed PostgreSQL. Connection handled via Environment Variables. |
| **Object Storage** | **Appwrite** | **Strictly required** because Render's local filesystem is ephemeral. |
| **Backend Core** | C# / ASP.NET Core (.NET 8.0) | Layered architecture (Thin Controllers, DTOs, Atomic Transactions). |

---

## 🏗️ Architecture Overview

The codebase follows a strict Layered System Architecture to ensure maintainability and clean separation of concerns:

```text
Presentation Layer   →  Razor Views, CSS/JS, Global Exception Middleware
Controller Layer     →  Thin Controllers (HTTP Routing, Session Management, DTO Mapping only)
Mapping Layer        →  Static Extension Mappers (Clean Entity ↔ DTO transformations)
Service Layer        →  Core Services (Account, Audit, DamagedReport, FixedAsset, Notification, AppwriteStorage)
Data Transfer Layer  →  DTOs (Decoupled from EF Core Entities to prevent over-posting)
Data Layer           →  DigitalFormsSystemContext (EF Core 8.0)
Database Layer       →  Supabase PostgreSQL
Storage Layer        →  Appwrite (Secure file/image handling)
```

---

## Deployment Workflow (Render CI/CD)

The staging environment is fully automated. There is no manual deployment required.

1. **Developer** pushes code to the `staging` branch on GitHub.
2. **Render** detects the push via the connected Git repository.
3. **Render** executes the Build Command (installs dependencies, runs EF Core migrations).
4. **Render** executes the Start Command and spins up the new container.
5. **Live URL** is updated automatically.

### Render Build & Start Commands
*These are configured in the Render Dashboard under the Web Service settings.*

**Build Command:**
```bash
dotnet restore
dotnet publish -c Release -o out
# Run migrations automatically on build
dotnet ef database update --project DigitalFormsSystem.Core --startup-project DigitalFormsSystem
```

**Start Command:**
```bash
cd out && dotnet DigitalFormsSystem.dll
```

---

## 🔐 Environment Configuration (Render)

Because Render does not use a local `appsettings.json` file for sensitive data, all configurations are injected via **Environment Variables** in the Render Dashboard.

Ensure the following Environment Variables are set in your Render Web Service:

### 1. Database (Supabase)

| Render Environment Variable | Value / Format |
| :--- | :--- |
| `ConnectionStrings__DefaultConnection` | `Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_SUPABASE_PASSWORD;` |

### 2. Storage (Appwrite)

| Render Environment Variable | Value / Format |
| :--- | :--- |
| `StorageSettings__Appwrite__Endpoint` | `https://cloud.appwrite.io/v1` (or your self-hosted URL) |
| `StorageSettings__Appwrite__ProjectId` | `your_staging_project_id` |
| `StorageSettings__Appwrite__ApiKey` | `your_staging_api_key` |
| `StorageSettings__Appwrite__BucketId` | `your_staging_bucket_id` |

### 3. Application Settings

| Render Environment Variable | Value / Format |
| :--- | :--- |
| `AppSettings__ManagerEmployeeId` | `778` (Or the ID of the staging admin) |
| `AppSettings__MaxFileSizeMB` | `10` |
| `AllowedHosts` | `*` (Or your specific Render `.onrender.com` domain) |

> ⚠️ **Note on Double Underscores (`__`):** Render (and Linux environments) use `__` to represent nested JSON configuration keys (e.g., `ConnectionStrings__DefaultConnection` maps to `"ConnectionStrings": { "DefaultConnection": "..." }`).

---

## 💻 Local Development (Pointing to Staging)

If you need to debug locally but want to use the **Staging Supabase Database** and **Staging Appwrite Bucket**, follow these steps:

1. Open `DigitalFormsSystem/appsettings.Development.json` (or your local `appsettings.json`).
2. Add the exact same keys used in Render, but in standard JSON format:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_SUPABASE_PASSWORD;"
  },
  "StorageSettings": {
    "Appwrite": {
      "Endpoint": "https://cloud.appwrite.io/v1",
      "ProjectId": "your_staging_project_id",
      "ApiKey": "your_staging_api_key",
      "BucketId": "your_staging_bucket_id"
    }
  },
  "AppSettings": {
    "ManagerEmployeeId": 778,
    "MaxFileSizeMB": 10
  }
}
```
3. Run the app locally: 
   ```bash
   dotnet run --project DigitalFormsSystem
   ```

---

## ⚠️ Staging-Specific Limitations & Notes

1. **Ephemeral Filesystem:** Render containers are ephemeral. If the container restarts, any files saved to the local disk (`wwwroot/uploads`) will be lost. **This is why Appwrite is strictly enforced in the codebase for all file uploads.**
2. **Database Migrations:** If you add new EF Core models, ensure you generate the migration locally (`dotnet ef migrations add ...`) and commit it. The Render Build Command will apply it automatically.
3. **Data Seeding:** Staging data is synthetic. If the staging database is wiped, employee data must be re-seeded using the `DbInitializer` or by importing a SQL dump from the production team.

---

## 📌 Roadmap (Staging Focus)

- [ ] Validate multi-stage approval workflow in a cloud environment.
- [ ] Stress-test Supabase connection pooling under concurrent Render container loads.
- [ ] Implement automated integration tests in the CI pipeline before Render deployment.

---

## Project Author

**Natajimura**
* **GitHub:** [@CjConvento](https://github.com/CjConvento)
* **LinkedIn:** [Cyrenz Jonathan Convento](https://www.linkedin.com/in/cyrenz-jonathan-convento-650a931b7/)
* **Email:** [conventocj110@gmail.com](mailto:conventocj110@gmail.com)
