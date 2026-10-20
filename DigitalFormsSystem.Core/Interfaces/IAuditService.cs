using System.Threading.Tasks;
using DigitalFormsSystem.Core.Models;
using System.Collections.Generic;

namespace DigitalFormsSystem.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            string action,
            string? entityType = null,
            int? entityId = null,
            string? details = null);

        Task<List<AuditLog>> GetRecentLogsAsync(int limit = 100);
    }
}