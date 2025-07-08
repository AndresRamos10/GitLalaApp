namespace LalaHealthCare.App.Models;

public class NotificationRequestModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduleTime { get; set; }
    public string? VisitId { get; set; }
    public NotificationType Type { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}

public enum NotificationType
{
    VisitReminder,      // Recordatorio de visita próxima
    VisitStart,         // Es hora de iniciar la visita
    VisitLate,          // Visita retrasada
    DailySummary,       // Resumen diario
    Emergency,          // Notificación de emergencia
    General             // Notificación general
}

public class NotificationSettingsModel
{
    public bool Enabled { get; set; } = true;
    public int ReminderMinutesBefore { get; set; } = 30; // Recordar 30 min antes
    public bool SoundEnabled { get; set; } = true;
    public bool VibrationEnabled { get; set; } = true;
    public TimeSpan QuietHoursStart { get; set; } = new TimeSpan(22, 0, 0); // 10 PM
    public TimeSpan QuietHoursEnd { get; set; } = new TimeSpan(7, 0, 0); // 7 AM
}