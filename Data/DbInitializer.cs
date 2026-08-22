using DigitalFormsSystem.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; 

namespace DigitalFormsSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedEmployeePasswords(DigitalFormsSystemContext context)
        {
            var employees = await context.Employees
                .Where(e => e.IsActive == true && e.PasswordHash == null)
                .ToListAsync();

            if (!employees.Any())
            {
                Console.WriteLine("✅ All employees already have password hashes.");
                return;
            }

            Console.WriteLine($"⏳ Seeding passwords for {employees.Count} employees...");

            foreach (var emp in employees)
            {
                var defaultPassword = $"HST{emp.EmployeeNo}!";
                emp.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);                
                emp.IsFirstLogin = true;
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Password seeding completed! {employees.Count} employees updated.");
        }
    }
}