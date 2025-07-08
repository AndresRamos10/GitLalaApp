using LalaHealthCare.App.Extensions;
using LalaHealthCare.App.Models;
using LalaHealthCare.App.Models.Enum;
using LalaHealthCare.DataAccess.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.EventArgs;

namespace LalaHealthCare.App.Services;

public class NotificationService : INotificationService
{
    private readonly ILoggingService _loggingService;
    private NotificationSettingsModel _settings;
    private INavigationService _navigation;
    private const string SETTINGS_KEY = "notification_settings";

    public NotificationService(ILoggingService loggingService, INavigationService navigation)
    {
        _loggingService = loggingService;
        _navigation = navigation;
        _settings = new NotificationSettingsModel();
        Initialize();
    }

    public void Initialize()
    {
        try
        {
            // Configurar el centro de notificaciones
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;

            // Cargar configuraciones guardadas
            _ = LoadSettingsAsync();
        }
        catch (Exception ex)
        {
            _loggingService.LogErrorAsync("Error initializing notification service", ex).Wait();
        }
    }

    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            var permissionStatus = await LocalNotificationCenter.Current.RequestNotificationPermission();

            if (!permissionStatus)
            {
                await _loggingService.LogWarningAsync("Notification permission denied");
            }

            return permissionStatus;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error requesting notification permission", ex);
            return false;
        }
    }

    public async Task ScheduleVisitRemindersAsync(Visit visit)
    {
        try
        {
            if (!_settings.Enabled || visit.Status != VisitStatus.Planned)
                return;

            // Cancelar notificaciones anteriores para esta visita
            await CancelVisitRemindersAsync(visit.Id);

            var now = DateTime.Now;

            // Recordatorio X minutos antes
            var reminderTime = visit.ScheduledDateTime.AddMinutes(-_settings.ReminderMinutesBefore);
            if (reminderTime > now)
            {
                var reminderNotification = new NotificationRequest
                {
                    NotificationId = GetNotificationId(visit.Id, "reminder"),
                    Title = "Visit Reminder",
                    Description = $"You have an appointment with {visit.PatientName} in {_settings.ReminderMinutesBefore} minutes",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = reminderTime,
                        RepeatType = NotificationRepeat.No
                    },
                    CategoryType = NotificationCategoryType.Reminder,
                    Android = new AndroidOptions
                    {
                        ChannelId = "visit_reminders",
                        Priority = AndroidPriority.High,
                        VisibilityType = AndroidVisibilityType.Public,
                        VibrationPattern = _settings.VibrationEnabled ? new long[] { 0, 500, 500, 500 } : null,
                        ProgressBar = new AndroidProgressBar { IsIndeterminate = false }
                    },
                    ReturningData = SerializeVisitData(visit, NotificationType.VisitReminder)
                };

                if (_settings.SoundEnabled && !IsQuietHours(reminderTime))
                {
                    reminderNotification.Sound = DeviceInfo.Platform == DevicePlatform.Android
                        ? "notification_sound"
                        : "notification_sound.wav";
                }

                await LocalNotificationCenter.Current.Show(reminderNotification);
            }

            // Notificación a la hora exacta
            if (visit.ScheduledDateTime > now)
            {
                var startNotification = new NotificationRequest
                {
                    NotificationId = GetNotificationId(visit.Id, "start"),
                    Title = "Visiting Hour",
                    Description = $"It's time to visit {visit.PatientName} in {visit.Location}",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = visit.ScheduledDateTime,
                        RepeatType = NotificationRepeat.No
                    },
                    CategoryType = NotificationCategoryType.Alarm,
                    Android = new AndroidOptions
                    {
                        ChannelId = "visit_start",
                        Priority = AndroidPriority.Max,
                        VisibilityType = AndroidVisibilityType.Public,
                        VibrationPattern = _settings.VibrationEnabled ? new long[] { 0, 1000, 500, 1000 } : null,
                        TimeoutAfter = TimeSpan.FromMinutes(5),
                        ProgressBar = new AndroidProgressBar { IsIndeterminate = false },
                        IconLargeName = new AndroidIcon { ResourceName = "ic_notification_large"},
                        IconSmallName = new AndroidIcon { ResourceName = "ic_notification_small" }
                    },
                    BadgeNumber = 1,
                    ReturningData = SerializeVisitData(visit, NotificationType.VisitStart)
                };

                if (_settings.SoundEnabled)
                {
                    startNotification.Sound = DeviceInfo.Platform == DevicePlatform.Android
                        ? "notification_urgent"
                        : "notification_urgent.wav";
                }

                await LocalNotificationCenter.Current.Show(startNotification);
            }

            // Notificación si llega tarde (15 minutos después)
            var lateTime = visit.ScheduledDateTime.AddMinutes(15);
            if (lateTime > now)
            {
                var lateNotification = new NotificationRequest
                {
                    NotificationId = GetNotificationId(visit.Id, "late"),
                    Title = "⚠️ Delayed Visit",
                    Description = $"The visit with {visit.PatientName} is delayed. Please check in.",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = lateTime,
                        RepeatType = NotificationRepeat.No
                    },
                    CategoryType = NotificationCategoryType.Alarm,
                    Android = new AndroidOptions
                    {
                        ChannelId = "visit_late",
                        Priority = AndroidPriority.Max,
                        VisibilityType = AndroidVisibilityType.Public,
                        VibrationPattern = new long[] { 0, 500, 250, 500, 250, 500 },
                        Color = new AndroidColor(16711680), // Rojo
                        ProgressBar = new AndroidProgressBar { IsIndeterminate = true }
                    },
                    BadgeNumber = 1,
                    ReturningData = SerializeVisitData(visit, NotificationType.VisitLate)
                };

                await LocalNotificationCenter.Current.Show(lateNotification);
            }

            await _loggingService.LogInformationAsync($"Visit reminders scheduled for visit {visit.Id}");
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error scheduling visit reminders for {visit.Id}", ex);
        }
    }

    public async Task CancelVisitRemindersAsync(string visitId)
    {
        try
        {
            var notificationIds = new[]
            {
            GetNotificationId(visitId, "reminder"),
            GetNotificationId(visitId, "start"),
            GetNotificationId(visitId, "late")
        };

            foreach (var id in notificationIds)
            {
                LocalNotificationCenter.Current.Cancel(id);
            }

            await _loggingService.LogInformationAsync($"Cancelled all reminders for visit {visitId}");
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error cancelling visit reminders for {visitId}", ex);
        }
    }

    public async Task ShowInstantNotificationAsync(string title, string description, NotificationType type = NotificationType.General)
    {
        try
        {
            var notification = new NotificationRequest
            {
                NotificationId = new Random().Next(10000, 99999),
                Title = title,
                Description = description,
                CategoryType = GetCategoryType(type),
                Android = new AndroidOptions
                {
                    ChannelId = GetChannelId(type),
                    Priority = GetPriority(type),
                    VisibilityType = AndroidVisibilityType.Public,
                    VibrationPattern = _settings.VibrationEnabled ? new long[] { 0, 500 } : null
                },
                ReturningData = new Dictionary<string, string>
            {
                { "type", type.ToString() },
                { "timestamp", DateTime.Now.ToString("O") }
            }.SerializeToJson()
            };

            if (_settings.SoundEnabled && !IsQuietHours(DateTime.Now))
            {
                notification.Sound = "notification_sound";
            }

            await LocalNotificationCenter.Current.Show(notification);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error showing instant notification", ex);
        }
    }

    public async Task ScheduleDailyReminderAsync()
    {
        try
        {
            var dailyTime = DateTime.Today.AddHours(8); // 8 AM

            var dailyNotification = new NotificationRequest
            {
                NotificationId = 9999,
                Title = "Daily Summary",
                Description = "Check your scheduled visits for today",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = dailyTime,
                    RepeatType = NotificationRepeat.Daily
                },
                CategoryType = NotificationCategoryType.Reminder,
                Android = new AndroidOptions
                {
                    ChannelId = "daily_summary",
                    Priority = AndroidPriority.Default,
                    VisibilityType = AndroidVisibilityType.Private
                }
            };

            await LocalNotificationCenter.Current.Show(dailyNotification);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error scheduling daily reminder", ex);
        }
    }

    public async Task<NotificationSettingsModel> GetSettingsAsync()
    {
        await LoadSettingsAsync();
        return _settings;
    }

    public async Task SaveSettingsAsync(NotificationSettingsModel settings)
    {
        try
        {
            _settings = settings;
            var json = System.Text.Json.JsonSerializer.Serialize(settings);
            await SecureStorage.SetAsync(SETTINGS_KEY, json);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error saving notification settings", ex);
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var json = await SecureStorage.GetAsync(SETTINGS_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                _settings = System.Text.Json.JsonSerializer.Deserialize<NotificationSettingsModel>(json) ?? new NotificationSettingsModel();
            }
        }
        catch
        {
            _settings = new NotificationSettingsModel();
        }
    }

    private void OnNotificationActionTapped(NotificationActionEventArgs e)
    {
        try
        {
            if (e.Request?.ReturningData != null)
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(e.Request.ReturningData);

                if (data != null && data.TryGetValue("visitId", out var visitId))
                {
                    // Navegar a los detalles de la visita
                    Application.Current?.MainPage?.Dispatcher.Dispatch(async () =>
                    {
                        // TODO: Implementar navegación a detalles de visita
                        //await Shell.Current.GoToAsync($"//dashboard?visitId={visitId}");
                        _navigation.NavigateTo(ConstNavigator.Dashboard);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogErrorAsync("Error handling notification tap", ex).Wait();
        }
    }

    private int GetNotificationId(string visitId, string type)
    {
        // Generar un ID único basado en el visitId y el tipo
        var hash = $"{visitId}_{type}".GetHashCode();
        return Math.Abs(hash % 100000);
    }

    private bool IsQuietHours(DateTime time)
    {
        var currentTime = time.TimeOfDay;

        if (_settings.QuietHoursStart > _settings.QuietHoursEnd)
        {
            // Horario nocturno (ej: 22:00 - 07:00)
            return currentTime >= _settings.QuietHoursStart || currentTime <= _settings.QuietHoursEnd;
        }
        else
        {
            // Horario diurno
            return currentTime >= _settings.QuietHoursStart && currentTime <= _settings.QuietHoursEnd;
        }
    }

    private string SerializeVisitData(Visit visit, NotificationType type)
    {
        var data = new Dictionary<string, string>
    {
        { "visitId", visit.Id },
        { "patientName", visit.PatientName },
        { "location", visit.Location },
        { "scheduledTime", visit.ScheduledDateTime.ToString("O") },
        { "type", type.ToString() }
    };

        return System.Text.Json.JsonSerializer.Serialize(data);
    }

    private NotificationCategoryType GetCategoryType(NotificationType type) => type switch
    {
        NotificationType.VisitReminder => NotificationCategoryType.Reminder,
        NotificationType.VisitStart => NotificationCategoryType.Alarm,
        NotificationType.VisitLate => NotificationCategoryType.Alarm,
        NotificationType.Emergency => NotificationCategoryType.Alarm,
        _ => NotificationCategoryType.Status
    };

    private string GetChannelId(NotificationType type) => type switch
    {
        NotificationType.VisitReminder => "visit_reminders",
        NotificationType.VisitStart => "visit_start",
        NotificationType.VisitLate => "visit_late",
        NotificationType.Emergency => "emergency",
        NotificationType.DailySummary => "daily_summary",
        _ => "general"
    };

    private AndroidPriority GetPriority(NotificationType type) => type switch
    {
        NotificationType.VisitStart => AndroidPriority.Max,
        NotificationType.VisitLate => AndroidPriority.Max,
        NotificationType.Emergency => AndroidPriority.Max,
        NotificationType.VisitReminder => AndroidPriority.High,
        _ => AndroidPriority.Default
    };
}
