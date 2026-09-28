namespace Application;

public interface INotificationService
{
    Task<bool> SendNotification(string message);
}