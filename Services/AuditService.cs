using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DigitalFormsSystem.Services
{
    public class AuditService : IAuditService
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;

        public AuditService(
            DigitalFormsSystemContext context,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
        }

        public async Task LogAsync(
            string action,
            string? entityType = null,
            int? entityId = null,
            string? details = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = _currentUserService.EmployeeId,
                    UserName = _currentUserService.EmployeeName,
                    EmployeeNo = _currentUserService.EmployeeNo,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = details,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString(),
                    CreatedAt = DateTime.Now
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't break the application
                Console.WriteLine($"Audit log error: {ex.Message}");
            }
        }
    }
}