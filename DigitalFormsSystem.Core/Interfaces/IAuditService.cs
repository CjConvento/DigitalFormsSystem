using System.Threading.Tasks;

namespace DigitalFormsSystem.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            string action,
            string? entityType = null,
            int? entityId = null,
            string? details = null);
    }
}