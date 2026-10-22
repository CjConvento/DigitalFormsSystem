# DFS- Troubleshooting Guide

This document provides solutions to common issues encountered during the deployment, configuration, and daily operation of the Digital Forms System (DFS) on the HST On-Premise IIS Server.

---

## 1. IIS & Web Server Issues

### Issue: 500.19 Internal Server Error (Config Error)
**Symptom:** The browser displays a 500.19 error with an error code like `0x8007000d`.
**Cause:** The server is missing the required .NET 8 Hosting Bundle, or the `web.config` file is malformed.
**Solution:**
1. Ensure the [.NET 8.0 Hosting Bundle](https://microsoft.com) is installed on the server.
2. Restart IIS via an administrative PowerShell: `iisreset`.
3. Verify that the `web.config` file exists in the publish deployment folder (`C:\Publish\DigitalFormsSystem`).

### Issue: 502.5 Process Failure
**Symptom:** The site loads but immediately returns a 502.5 error.
**Cause:** The application crashed on startup due to runtime exceptions.
**Solution:**
1. Open **Event Viewer** (Windows Logs -> Application).
2. Look for errors from "IIS AspNetCore Module" or ".NET Runtime". It will specify the exact exception (e.g., database connection timeout).
3. Ensure the `appsettings.json` file is present in the publish folder and contains valid JSON syntax.

### Issue: 403 Forbidden / Access Denied
**Symptom:** Accessing the site returns a 403 error.
**Cause:** The IIS App Pool identity does not have read/write access permissions to the physical file path.
**Solution:**
Run the following command in an Administrator PowerShell to grant access:
```powershell
icacls C:\Publish\DigitalFormsSystem /grant "IIS AppPool\DFSApp:(OI)(CI)M"
```

---

## 2. Database & SQL Server Issues

### Issue: "Cannot open database requested by the login. Login failed for user 'IIS AppPool\DFSApp'."
**Symptom:** The app throws a SQL Exception on startup or during web authentication.
**Cause:** The IIS App Pool account identity has not been granted login mappings to the target database instance.
**Solution:**
Open **SSMS**, navigate to Security -> Logins, and ensure `IIS AppPool\DFSApp` is mapped to the `DigitalFormsSystem` database with `db_datareader` and `db_datawriter` database roles assigned.

### Issue: "A network-related or instance-specific error occurred while establishing a connection to SQL Server."
**Symptom:** The app cannot connect to the database.
**Cause:** Incorrect configuration keys, firewalls, or named database instances are misconfigured.
**Solution:**
1. Check `appsettings.json`. Ensure the key is exactly named **`DigitalFormsSystemContext`** (do not use generic keys like `DefaultConnection`).
2. Verify that the connection string parameters contain `Server=YOUR_SERVER_NAME\INSTANCE;` and include `Encrypt=True;TrustServerCertificate=True;` to bypass certificate verification boundaries over local area connections.

### Issue: EF Core Migration Fails on Production
**Symptom:** Running `dotnet ef database update` throws environment routing faults.
**Cause:** Production host servers usually only carry the .NET runtime environment layers, not the heavy developer .NET SDK compiler framework modules.
**Solution:**
Do not run EF Core CLI targets inside production perimeters. Utilize **Option B** from Step 6 of the deployment guide: load and execute the static `scripts/init_schema.sql` file natively inside SQL Server Management Studio (SSMS).

---

## 3. Local File Storage & Upload Issues

### Issue: "Access to the path 'C:\Publish\DigitalFormsSystem\wwwroot\uploads' is denied."
**Symptom:** Creating a Damaged Report or Fixed Asset Request with an attachment fails with an HTTP 500 fault.
**Cause:** The active worker process pool identity lacks active write directory privileges inside the assets folder.
**Solution:**
Run the following command in an Administrator PowerShell instance:
```powershell
icacls C:\Publish\DigitalFormsSystem\wwwroot\uploads /grant "IIS AppPool\DFSApp:(OI)(CI)M"
```

### Issue: 404.13 Content Length Too Large (File Upload Fails for files > 30MB)
**Symptom:** Uploading large image files (up to the 100MB system threshold constraint) fails immediately.
**Cause:** The IIS native configuration baseline enforces a global limit of 30MB (`maxAllowedContentLength`), which overrides your internal .NET application configurations.
**Solution:**
Ensure the auto-generated `web.config` profile folder contains explicit overrides inside the `<system.webServer>` elements:
```xml
<security>
  <requestFiltering>
    <!-- Configures IIS to allow up to 100 MB in bytes -->
    <requestLimits maxAllowedContentLength="104857600" />
  </requestFiltering>
</security>
```

---

## 4. Application & Identity Pipeline Issues

### Issue: First Login Takes 3-5 Seconds (Cold Start)
**Symptom:** The first time the site is loaded after an IIS recycle or server hotfix, loading speeds stall briefly. Subsequent operations run in milliseconds.
**Cause:** Standard .NET execution pattern framework architecture behavior. The machine performs Just-In-Time (JIT) compilation passes and hydrates the Entity Framework metadata arrays.
**Solution:** No dynamic code mitigation is required. If necessary, the administrator can configure the IIS Application Pool's **Start Mode** parameter flag to `AlwaysRunning` and change the Site's metadata property to **Preload Enabled**.

### Issue: Employees Cannot Login (Invalid Credentials)
**Symptom:** Migrated workers cannot authenticate using temporary onboarding data.
**Cause:** The browser connection hasn't initialized the pipeline, or the database migration bypassed the operational synchronization scope metrics check loops.
**Solution:**
1. Ensure the web application URL endpoint has been hit at least once after running Step 6.5. The database seeding process hooks into the application's first cold start event transaction context lifecycle.
2. Confirm that the `employee_credentials_report.pdf` document asset compiled inside the root publish folder path. If the directory is blank, check the server log profiles to ensure rows returning `PasswordHash == null` exist in the database.

### Issue: "The process cannot access the file... because it is being used by another process."
**Symptom:** Running hotfixes or transferring code updates throws file lock faults.
**Cause:** The operational IIS worker process engine handles active threads on code binaries while the application instance is live.
**Solution:**
Never manually override running publish target paths. Always execute the provided deployment pipeline wrappers (`scripts/redeploy.bat`), which safely handle application pool termination states, hot-swapping assets, and automatic clean server recycling tasks.
