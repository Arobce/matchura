using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Hubs;

namespace NotificationService.Infrastructure.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly NotificationDbContext _db;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationServiceImpl> _logger;

    public NotificationServiceImpl(
        NotificationDbContext db,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationServiceImpl> logger)
    {
        _db = db;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<NotificationListResponse> GetNotificationsAsync(string userId, int page, int pageSize)
    {
        var query = _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new NotificationListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<UnreadCountResponse> GetUnreadCountAsync(string userId)
    {
        var count = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return new UnreadCountResponse { Count = count };
    }

    public async Task MarkAsReadAsync(string userId, Guid notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

        if (notification == null) return;

        notification.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task CreateNotificationAsync(
        string userId, string type, string title, string message,
        string? relatedEntityId = null, string? relatedEntityType = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        var response = MapToResponse(notification);

        // Push real-time via SignalR
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", response);

        _logger.LogInformation("Notification sent to {UserId}: {Type}", userId, type);
    }

    private static NotificationResponse MapToResponse(Notification n) => new()
    {
        NotificationId = n.NotificationId,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        RelatedEntityId = n.RelatedEntityId,
        RelatedEntityType = n.RelatedEntityType,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
