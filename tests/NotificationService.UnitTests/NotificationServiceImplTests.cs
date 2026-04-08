using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Hubs;
using NotificationService.Infrastructure.Services;
using Shared.TestUtilities;

namespace NotificationService.UnitTests;

public class NotificationServiceImplTests : IDisposable
{
    private readonly NotificationDbContext _db;
    private readonly SqliteConnection _connection;
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly NotificationServiceImpl _sut;

    private const string UserId = "user-123";
    private const string OtherUserId = "user-999";

    public NotificationServiceImplTests()
    {
        (_db, _connection) = DbContextFactory.Create<NotificationDbContext>();

        // SQLite does not support gen_random_uuid(), so assign GUIDs client-side before saving.
        _db.SavingChanges += (sender, _) =>
        {
            if (sender is not NotificationDbContext ctx) return;
            foreach (var entry in ctx.ChangeTracker.Entries<Notification>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.NotificationId == Guid.Empty)
                    entry.Entity.NotificationId = Guid.NewGuid();
            }
        };

        var mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var logger = new Mock<ILogger<NotificationServiceImpl>>();
        _sut = new NotificationServiceImpl(_db, _mockHubContext.Object, logger.Object);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // --- Helper ---

    private async Task<Notification> SeedNotification(
        string userId,
        bool isRead = false,
        DateTime? createdAt = null,
        string type = "info",
        string title = "Test",
        string message = "Test message")
    {
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            IsRead = isRead,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
        return notification;
    }

    // =============================================
    // CreateNotificationAsync
    // =============================================

    [Fact]
    public async Task CreateNotificationAsync_SavesNotificationToDatabase()
    {
        await _sut.CreateNotificationAsync(UserId, "job_match", "New Match", "You have a new match!");

        var saved = _db.Notifications.SingleOrDefault();
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(UserId);
        saved.Type.Should().Be("job_match");
        saved.Title.Should().Be("New Match");
        saved.Message.Should().Be("You have a new match!");
        saved.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task CreateNotificationAsync_SavesOptionalRelatedEntityFields()
    {
        await _sut.CreateNotificationAsync(
            UserId, "job_match", "Match", "Details",
            relatedEntityId: "job-42", relatedEntityType: "Job");

        var saved = _db.Notifications.Single();
        saved.RelatedEntityId.Should().Be("job-42");
        saved.RelatedEntityType.Should().Be("Job");
    }

    [Fact]
    public async Task CreateNotificationAsync_PushesNotificationViaSignalR()
    {
        await _sut.CreateNotificationAsync(UserId, "info", "Title", "Body");

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object?[]>(args => args.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateNotificationAsync_SendsToCorrectUserGroup()
    {
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.Group(UserId)).Returns(mockClientProxy.Object);
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        await _sut.CreateNotificationAsync(UserId, "info", "Title", "Body");

        mockClients.Verify(c => c.Group(UserId), Times.Once);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithoutOptionalFields_SetsNulls()
    {
        await _sut.CreateNotificationAsync(UserId, "info", "Title", "Body");

        var saved = _db.Notifications.Single();
        saved.RelatedEntityId.Should().BeNull();
        saved.RelatedEntityType.Should().BeNull();
    }

    // =============================================
    // GetNotificationsAsync
    // =============================================

    [Fact]
    public async Task GetNotificationsAsync_ReturnsOnlyRequestedUsersNotifications()
    {
        await SeedNotification(UserId);
        await SeedNotification(UserId);
        await SeedNotification(OtherUserId);

        var result = await _sut.GetNotificationsAsync(UserId, 1, 10);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetNotificationsAsync_OrdersByCreatedAtDescending()
    {
        var older = await SeedNotification(UserId, createdAt: new DateTime(2025, 1, 1));
        var newer = await SeedNotification(UserId, createdAt: new DateTime(2025, 6, 1));

        var result = await _sut.GetNotificationsAsync(UserId, 1, 10);

        result.Items[0].NotificationId.Should().Be(newer.NotificationId);
        result.Items[1].NotificationId.Should().Be(older.NotificationId);
    }

    [Fact]
    public async Task GetNotificationsAsync_PaginatesCorrectly()
    {
        for (int i = 0; i < 5; i++)
            await SeedNotification(UserId, createdAt: DateTime.UtcNow.AddMinutes(i));

        var page1 = await _sut.GetNotificationsAsync(UserId, 1, 2);
        var page2 = await _sut.GetNotificationsAsync(UserId, 2, 2);
        var page3 = await _sut.GetNotificationsAsync(UserId, 3, 2);

        page1.Items.Should().HaveCount(2);
        page2.Items.Should().HaveCount(2);
        page3.Items.Should().HaveCount(1);
        page1.TotalCount.Should().Be(5);
        page1.TotalPages.Should().Be(3);
        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetNotificationsAsync_ClampsPageSizeToMax50()
    {
        await SeedNotification(UserId);

        var result = await _sut.GetNotificationsAsync(UserId, 1, 100);

        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetNotificationsAsync_ClampsPageSizeToMin1()
    {
        await SeedNotification(UserId);

        var result = await _sut.GetNotificationsAsync(UserId, 1, 0);

        result.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task GetNotificationsAsync_ClampsPageToMin1()
    {
        await SeedNotification(UserId);

        var result = await _sut.GetNotificationsAsync(UserId, -1, 10);

        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetNotificationsAsync_ReturnsEmptyForUserWithNoNotifications()
    {
        await SeedNotification(OtherUserId);

        var result = await _sut.GetNotificationsAsync(UserId, 1, 10);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetNotificationsAsync_MapsAllFieldsCorrectly()
    {
        var n = await SeedNotification(UserId, type: "job_match", title: "Matched",
            message: "You matched", isRead: true);

        var result = await _sut.GetNotificationsAsync(UserId, 1, 10);
        var item = result.Items.Single();

        item.NotificationId.Should().Be(n.NotificationId);
        item.Type.Should().Be("job_match");
        item.Title.Should().Be("Matched");
        item.Message.Should().Be("You matched");
        item.IsRead.Should().BeTrue();
    }

    // =============================================
    // GetUnreadCountAsync
    // =============================================

    [Fact]
    public async Task GetUnreadCountAsync_CountsOnlyUnreadForUser()
    {
        await SeedNotification(UserId, isRead: false);
        await SeedNotification(UserId, isRead: false);
        await SeedNotification(UserId, isRead: true);
        await SeedNotification(OtherUserId, isRead: false);

        var result = await _sut.GetUnreadCountAsync(UserId);

        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsZeroWhenAllRead()
    {
        await SeedNotification(UserId, isRead: true);
        await SeedNotification(UserId, isRead: true);

        var result = await _sut.GetUnreadCountAsync(UserId);

        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsZeroForUserWithNoNotifications()
    {
        var result = await _sut.GetUnreadCountAsync(UserId);

        result.Count.Should().Be(0);
    }

    // =============================================
    // MarkAsReadAsync
    // =============================================

    [Fact]
    public async Task MarkAsReadAsync_MarksSpecificNotificationAsRead()
    {
        var n = await SeedNotification(UserId, isRead: false);

        await _sut.MarkAsReadAsync(UserId, n.NotificationId);

        var updated = _db.Notifications.Single(x => x.NotificationId == n.NotificationId);
        updated.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsReadAsync_DoesNotAffectOtherNotifications()
    {
        var target = await SeedNotification(UserId, isRead: false);
        var other = await SeedNotification(UserId, isRead: false);

        await _sut.MarkAsReadAsync(UserId, target.NotificationId);

        var otherUpdated = _db.Notifications.Single(x => x.NotificationId == other.NotificationId);
        otherUpdated.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsReadAsync_DoesNothingWhenNotificationNotFound()
    {
        var n = await SeedNotification(UserId, isRead: false);

        await _sut.MarkAsReadAsync(UserId, Guid.NewGuid());

        var unchanged = _db.Notifications.Single(x => x.NotificationId == n.NotificationId);
        unchanged.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsReadAsync_DoesNothingWhenUserIdDoesNotMatch()
    {
        var n = await SeedNotification(UserId, isRead: false);

        await _sut.MarkAsReadAsync(OtherUserId, n.NotificationId);

        var unchanged = _db.Notifications.Single(x => x.NotificationId == n.NotificationId);
        unchanged.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsReadAsync_IsIdempotentOnAlreadyReadNotification()
    {
        var n = await SeedNotification(UserId, isRead: true);

        await _sut.MarkAsReadAsync(UserId, n.NotificationId);

        var updated = _db.Notifications.Single(x => x.NotificationId == n.NotificationId);
        updated.IsRead.Should().BeTrue();
    }

    // =============================================
    // MarkAllAsReadAsync
    // =============================================

    [Fact]
    public async Task MarkAllAsReadAsync_MarksAllUnreadNotificationsForUser()
    {
        await SeedNotification(UserId, isRead: false);
        await SeedNotification(UserId, isRead: false);
        await SeedNotification(UserId, isRead: true);

        await _sut.MarkAllAsReadAsync(UserId);

        // Detach all tracked entities so we get fresh data from DB
        foreach (var entry in _db.ChangeTracker.Entries().ToList())
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        var notifications = _db.Notifications.Where(n => n.UserId == UserId).ToList();
        notifications.Should().HaveCount(3);
        notifications.Should().AllSatisfy(n => n.IsRead.Should().BeTrue());
    }

    [Fact]
    public async Task MarkAllAsReadAsync_DoesNotAffectOtherUsers()
    {
        await SeedNotification(UserId, isRead: false);
        await SeedNotification(OtherUserId, isRead: false);

        await _sut.MarkAllAsReadAsync(UserId);

        foreach (var entry in _db.ChangeTracker.Entries().ToList())
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        var otherNotification = _db.Notifications.Single(n => n.UserId == OtherUserId);
        otherNotification.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_DoesNothingWhenNoUnreadExist()
    {
        await SeedNotification(UserId, isRead: true);

        var act = async () => await _sut.MarkAllAsReadAsync(UserId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_DoesNothingForUserWithNoNotifications()
    {
        var act = async () => await _sut.MarkAllAsReadAsync("nonexistent-user");

        await act.Should().NotThrowAsync();
    }
}
