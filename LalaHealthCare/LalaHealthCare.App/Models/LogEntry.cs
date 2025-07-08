namespace LalaHealthCare.App.Models;

public class LogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int? UserId { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
    public string? DeviceInfo { get; set; }
    public string? AppVersion { get; set; }
}

public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}
