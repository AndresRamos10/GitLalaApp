using LalaHealthCare.App.Models;
using LalaHealthCare.DataAccess.Models;

namespace LalaHealthCare.App.Services;

public interface INotificationService
{
    Task<bool> RequestPermissionAsync();
    Task ScheduleVisitRemindersAsync(Visit visit);
    Task CancelVisitRemindersAsync(string visitId);
    Task ShowInstantNotificationAsync(string title, string description, NotificationType type = NotificationType.General);
    Task ScheduleDailyReminderAsync();
    Task<NotificationSettingsModel> GetSettingsAsync();
    Task SaveSettingsAsync(NotificationSettingsModel settings);
    void Initialize();
}

