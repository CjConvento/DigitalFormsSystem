namespace DigitalFormsSystem.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendSmsAsync(string phoneNumber, string message);
        Task NotifyStatusChangeAsync(
            int requestId, 
            string? oldStatus,   
            string? newStatus,   
            int changedByEmployeeId);
    }
}