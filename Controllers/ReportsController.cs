using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DigitalFormsSystem.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly int _managerId;

        // ✅ Strongly-typed DTO
        private class EmployeeCredentialDto
        {
            public int Id { get; set; }
            public string? EmployeeNo { get; set; }
            public string? Name { get; set; }
            public string? Department { get; set; }
            public string? Location { get; set; }
            public string? Section { get; set; }
        }

        public ReportsController(
            DigitalFormsSystemContext context,
            ICurrentUserService currentUserService,
            IConfiguration configuration)
        {
            _context = context;
            _currentUserService = currentUserService;
            _managerId = configuration.GetValue<int>("AppSettings:ManagerEmployeeId");
        }

        public async Task<IActionResult> EmployeePasswordsPdf()
        {
            Console.WriteLine("🔍 EmployeePasswordsPdf action called!");
            
            if (_currentUserService.EmployeeId != _managerId)
            {
                Console.WriteLine("❌ Not authorized!");
                TempData["ErrorMessage"] = "You are not authorized to view employee credentials.";
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine("✅ Authorized! Generating PDF...");
            
            var employees = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.EmployeeNo)
                .Select(e => new EmployeeCredentialDto
                {
                    Id = e.Id,
                    EmployeeNo = e.EmployeeNo,
                    Name = e.Name,
                    Department = e.Department,
                    Location = e.Location,
                    Section = e.Section
                })
                .ToListAsync();

            Console.WriteLine($"📊 Found {employees.Count} employees");

            var pdfStream = GenerateEmployeeCredentialsPdf(employees);
            Console.WriteLine("✅ PDF generated! Returning file...");

            return File(pdfStream, "application/pdf", "Employee_Initial_Passwords.pdf");
        }

        
        private MemoryStream GenerateEmployeeCredentialsPdf(List<EmployeeCredentialDto> employees)
        {
            var stream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4.Landscape());

                    // HEADER
                    page.Header().Column(col =>
                    {
                        col.Item().Text("HST DIGITAL FORMS SYSTEM").FontSize(18).Bold().AlignCenter().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("EMPLOYEE CREDENTIALS REPORT").FontSize(14).Bold().AlignCenter();
                        col.Item().Text("⚠️ FOR DEMO / TESTING PURPOSES ONLY ⚠️").FontSize(11).AlignCenter().FontColor(Colors.Red.Medium);
                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}").FontSize(9).AlignRight().FontColor(Colors.Grey.Medium);
                    });

                    // TABLE
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);   // ID
                            columns.RelativeColumn(2);   // Employee No.
                            columns.RelativeColumn(3);   // Name
                            columns.RelativeColumn(2);   // Department
                            columns.RelativeColumn(1);   // Location
                            columns.RelativeColumn(2);   // Section
                            columns.RelativeColumn(3);   // Default Password
                        });

                        // HEADER
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("ID").Bold().AlignCenter().FontColor(Colors.White).FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Employee No.").Bold().AlignCenter().FontColor(Colors.White).FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Employee Name").Bold().AlignLeft().FontColor(Colors.White).FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Department").Bold().AlignCenter().FontColor(Colors.White).FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Location").Bold().AlignCenter().FontColor(Colors.White).FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Section").Bold().AlignLeft().FontColor(Colors.White).FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Default Password").Bold().AlignCenter().FontColor(Colors.White).FontSize(9);
                        });

                        // DATA ROWS
                        int rowNumber = 0;
                        foreach (var emp in employees)
                        {
                            rowNumber++;
                            var defaultPassword = $"HST{emp.EmployeeNo}!";
                            var isManager = emp.Id == 778;
                            var bgColor = rowNumber % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                            if (isManager) bgColor = Colors.Yellow.Lighten2;

                            table.Cell().Background(bgColor).Padding(3).Text(emp.Id.ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(emp.EmployeeNo ?? "").AlignCenter().FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(emp.Name ?? "N/A").AlignLeft().FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(emp.Department ?? "N/A").AlignCenter().FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(emp.Location ?? "N/A").AlignCenter().FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(emp.Section ?? "N/A").AlignLeft().FontSize(8);
                            
                            // Password column with special styling
                            var cell = table.Cell().Background(bgColor).Padding(3);
                            if (isManager)
                            {
                                cell.Text(defaultPassword).AlignCenter().FontSize(8).FontColor(Colors.Red.Darken2).Bold();
                            }
                            else
                            {
                                cell.Text(defaultPassword).AlignCenter().FontSize(8).FontColor(Colors.Green.Darken2);
                            }
                        }
                    });

                    // FOOTER
                    page.Footer().Column(col =>
                    {
                        col.Item().Text($"Generated by: {_currentUserService.EmployeeName ?? "System Admin"} | {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                            .AlignCenter().FontSize(8).FontColor(Colors.Grey.Medium);

                        col.Item().Text("This document contains confidential information. For authorized use only.")
                            .AlignCenter().FontSize(7).FontColor(Colors.Red.Medium);
                    });
                });
            }).GeneratePdf(stream);

            stream.Position = 0;
            return stream;
        }
    }
}