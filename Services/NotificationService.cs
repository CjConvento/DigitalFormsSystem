using DigitalFormsSystem.Core.Interfaces;

namespace DigitalFormsSystem.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Log a stable identifier, not the actual recipient
            // If you have recipient user ID, pass it in. Otherwise, log a hash or "unspecified".
            _logger.LogInformation("Email notification dispatched.");
            _logger.LogDebug("Email subject length: {SubjectLength}", subject?.Length ?? 0);
            await Task.CompletedTask;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            // TODO: Integrate with actual SMS service (Twilio, etc.)
            // For now, just log
            _logger.LogInformation("SMS notification dispatched.");
            await Task.CompletedTask;
        }

        public Task NotifyStatusChangeAsync(int requestId, string? oldStatus, string? newStatus, int changedByEmployeeId)
        {
            _logger.LogInformation(
                "Status changed for request {RequestId}: {OldStatus} → {NewStatus} by employee {EmployeeId}",
                requestId, oldStatus, newStatus, changedByEmployeeId);
            return Task.CompletedTask;
        }
    }
}