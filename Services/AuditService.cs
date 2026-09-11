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
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            DigitalFormsSystemContext context,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService,
            ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
            _logger = logger;    
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
                _logger.LogError("Audit log write failed for action {Action} on {EntityType}.", action, entityType);
                _logger.LogDebug(ex, "Audit log exception details");
            }
        }
    }
}