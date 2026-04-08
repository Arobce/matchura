using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationListResponse> GetNotificationsAsync(string userId, int page, int pageSize);
    Task<UnreadCountResponse> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(string userId, Guid notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task CreateNotificationAsync(string userId, string type, string title, string message, string? relatedEntityId = null, string? relatedEntityType = null);
}
