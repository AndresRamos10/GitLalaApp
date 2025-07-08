using LalaHealthCare.App.Models;

namespace LalaHealthCare.App.Services;

public interface ILoggingService
{
    Task LogAsync(LogLevel level, string message, string? category = null, Exception? exception = null, Dictionary<string, object>? additionalData = null);
    Task LogErrorAsync(string message, Exception? exception = null, Dictionary<string, object>? additionalData = null);
    Task LogWarningAsync(string message, Dictionary<string, object>? additionalData = null);
    Task LogInformationAsync(string message, Dictionary<string, object>? additionalData = null);
    Task<List<LogEntry>> GetLogsAsync(int count = 100);
    Task ClearLogsAsync();
}
