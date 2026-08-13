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
            // TODO: Integrate with actual email service (SendGrid, SMTP, etc.)
            // For now, just log
            _logger.LogInformation($"Email sent to {to}: {subject}");
            await Task.CompletedTask;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            // TODO: Integrate with actual SMS service (Twilio, etc.)
            // For now, just log
            _logger.LogInformation($"SMS sent to {phoneNumber}: {message}");
            await Task.CompletedTask;
        }

        public async Task NotifyStatusChangeAsync(int requestId, string? oldStatus, string? newStatus, int changedByEmployeeId)
        {
            // TODO: Determine who to notify based on status change
            // For now, just log
            _logger.LogInformation($"Status changed: Request {requestId}: {oldStatus} → {newStatus} by Employee {changedByEmployeeId}");
            await Task.CompletedTask;
        }
    }
}