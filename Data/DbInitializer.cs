using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DigitalFormsSystem.Core.Models;
using DigitalFormsSystem.Core.Helpers; // Assuming PasswordGenerator is here
using BCrypt.Net;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DigitalFormsSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedEmployeePasswords(DigitalFormsSystemContext context)
        {
            // 1. Fetch only active employees who do not have password hashes yet
            var employees = await context.Employees
                .Where(e => e.IsActive == true && e.PasswordHash == null)
                .ToListAsync();

            if (!employees.Any())
            {
                Console.WriteLine("✅ All employees already have password hashes.");
                return;
            }

            Console.WriteLine($"⏳ Seeding passwords for {employees.Count} employees...");

            // 2. Initialize a transient list in RAM to store the plain-text passwords for the PDF
            var pdfReportData = new List<EmployeeOnboardingModel>();

            foreach (var emp in employees)
            {
                // 3. Generate the plain-text password dynamically in memory
                var defaultPassword = PasswordGenerator.GeneratePasswordWithEmployeeNo(emp.EmployeeNo, 8);

                // 4. Cryptographically hash it using BCrypt and bind it to the tracking entity
                emp.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

                // 5. Intercept the transient plain-text values and capture them in the RAM model
                pdfReportData.Add(new EmployeeOnboardingModel
                {
                    ID = emp.Id, // Matches your existing lowercase property casing
                    EmployeeNo = emp.EmployeeNo,
                    Name = emp.Name,
                    Department = emp.Department,
                    Location = emp.Location,
                    Section = emp.Section,
                    PlainTextPassword = defaultPassword // HELD STRICLY IN ACTIVE RAM
                });
            }

            // 6. Persist the secure BCrypt hashes to SQL Server (No plaintext columns touched)
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Password seeding completed! {employees.Count} employees updated in the database.");


            try
            {
                // 👑 CRITICAL ENVIRONMENT PATH REFACTOR: Bridges local worker pool system execution restrictions
                string baseFolder = @"C:\Temp";
        
                // Fallback hardcoded path  
                if (!Directory.Exists(baseFolder))
                {
                    baseFolder = @"C:\inetpub\wwwroot\DFSApp\wwwroot\uploads";
                }

                string outputFile = Path.Combine(baseFolder, "employee_credentials_report.pdf");

                string errorLogFile = Path.Combine(baseFolder, "questpdf_debug_log.txt");
                File.WriteAllText(errorLogFile, $"[Tracing] Target Compilation Started: {DateTime.Now}\nTarget File Location: {outputFile}\nRows Counted: {pdfReportData.Count}");

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1, Unit.Centimetre);

                        // ============ HEADER SECTION LAYER ============
                        page.Header().Column(headerCol =>
                        {
                            headerCol.Item().Text("HST DFS: FIXED ASSETS & DAMAGED REPORTS INTERNAL TOOL")
                                .Bold().FontSize(16).FontColor("#003366").AlignCenter();

                            headerCol.Item().Text("EMPLOYEE CREDENTIALS REPORT")
                                .SemiBold().FontSize(12).FontColor("#003366").AlignCenter();

                            headerCol.Item().PaddingTop(2).Text("⚠️ FOR ADMIN ONLY ⚠️")
                                .Bold().FontSize(10).FontColor(Colors.Red.Medium).AlignCenter();

                            headerCol.Item().PaddingTop(4).Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                                .FontSize(8).FontColor(Colors.Grey.Medium).AlignRight();
                        });

                        // ============ CORE DATA GRID PRESENTATION LAYER ============
                        page.Content().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.8f); // ID
                                columns.RelativeColumn(1.4f); // Employee No.
                                columns.RelativeColumn(2.2f); // Employee Name
                                columns.RelativeColumn(1.2f); // Department
                                columns.RelativeColumn(0.8f); // Location
                                columns.RelativeColumn(1.6f); // Section
                                columns.RelativeColumn(2.0f); // Default Password
                            });

                            // Structured Table Headers
                            table.Header(header =>
                            {
                                string headerBg = "#003366";
                                header.Cell().Background(headerBg).Padding(4).AlignCenter().Text("ID").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(headerBg).Padding(4).AlignCenter().Text("Employee No.").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(headerBg).Padding(4).AlignLeft().Text("Employee Name").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(headerBg).Padding(4).AlignCenter().Text("Department").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(headerBg).Padding(4).AlignCenter().Text("Location").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(headerBg).Padding(4).AlignLeft().Text("Section").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(headerBg).Padding(4).AlignCenter().Text("Default Password").FontColor(Colors.White).FontSize(8).Bold();
                            });

                            // Populate rows with dynamic alternating background configurations
                            int rowCounter = 0;
                            foreach (var row in pdfReportData)
                            {
                                rowCounter++;
                                bool isManager = row.ID.ToString() == "778";

                                // Replicates Python's white-whitesmoke pattern layout rules
                                string rowBg = (rowCounter % 2 == 0) ? Colors.Grey.Lighten4 : Colors.White;

                                // Explicitly highlight Administrative Row footprints (Hex: #FFF3CD)
                                if (isManager) rowBg = "#FFF3CD";

                                table.Cell().Background(rowBg).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).AlignCenter().Text(row.ID.ToString()).FontSize(7);
                                table.Cell().Background(rowBg).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).AlignCenter().Text(row.EmployeeNo ?? "").FontSize(7);
                                table.Cell().Background(rowBg).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).AlignLeft().Text(row.Name ?? "").FontSize(7);
                                table.Cell().Background(rowBg).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).AlignCenter().Text(row.Department ?? "").FontSize(7);
                                table.Cell().Background(rowBg).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).AlignCenter().Text(row.Location ?? "").FontSize(7);
                                table.Cell().Background(rowBg).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).AlignLeft().Text(row.Section ?? "").FontSize(7);

                                var passwordCell = table.Cell().Background(rowBg).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).AlignCenter();
                                if (isManager)
                                {
                                    passwordCell.Text($"⭐ {row.PlainTextPassword}").Bold().FontColor("#CC0000").FontSize(7);
                                }
                                else
                                {
                                    passwordCell.Text(row.PlainTextPassword ?? "").FontColor("#006600").FontSize(7);
                                }
                            }
                        });

                        page.Footer().Column(footerCol =>
                        {
                            footerCol.Item().PaddingTop(8).Text(text =>
                            {
                                text.Span("⭐ ").FontColor("#CC0000");
                                text.Span("Admin (ID 778) ").Bold().FontSize(9);
                                text.Span("— System Administrator / IT Manager").Italic().FontSize(9);
                            });

                            footerCol.Item().PaddingTop(4).Text($"Total Active Employees: {pdfReportData.Count}")
                                .Bold().FontSize(9).FontColor("#003366");

                            footerCol.Item().PaddingTop(4).Text("This document contains confidential information. For authorized use only.")
                                .FontSize(7).FontColor(Colors.Grey.Medium).AlignCenter();
                        });
                    });
                })
                .GeneratePdf(outputFile);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"📦 Onboarding dossier successfully compiled and exported to file system: {outputFile}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ QuestPDF Crash Intercepted: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                }
                Console.ResetColor();
            }
        }
    }

    // High-performance helper transfer template used exclusively by RAM processing layers
    public class EmployeeOnboardingModel
    {
        public int ID { get; set; }
        public string? EmployeeNo { get; set; }
        public string? Name { get; set; }
        public string? Department { get; set; }
        public string? Section { get; set; }
        public string? Location { get; set; }
        public string? PlainTextPassword { get; set; }
    }
}
