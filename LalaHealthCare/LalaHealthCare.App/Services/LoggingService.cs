using LalaHealthCare.App.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LogLevel = LalaHealthCare.App.Models.LogLevel;

namespace LalaHealthCare.App.Services;

public class LoggingService : ILoggingService
{
    private readonly List<LogEntry> _localLogs = new();
    private readonly AppState _appState;
    private readonly ILogger<LoggingService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public LoggingService(AppState appState, ILogger<LoggingService> logger)
    {
        _appState = appState;
        _logger = logger;
    }

    public async Task LogAsync(LogLevel level, string message, string? category = null, Exception? exception = null, Dictionary<string, object>? additionalData = null)
    {
        var logEntry = new LogEntry
        {
            Level = level,
            Message = message,
            Category = category ?? "General",
            UserId = _appState.CurrentUser?.Id,
            StackTrace = exception?.StackTrace,
            AdditionalData = additionalData,
            DeviceInfo = GetDeviceInfo(),
            AppVersion = GetAppVersion()
        };

        // Guardar localmente
        await SaveLogLocallyAsync(logEntry);

        // Simular envío a API
        await SendLogToApiAsync(logEntry);

        // También log en la consola para debugging
        LogToConsole(logEntry, exception);
    }

    public Task LogErrorAsync(string message, Exception? exception = null, Dictionary<string, object>? additionalData = null)
    {
        return LogAsync(LogLevel.Error, message, "Error", exception, additionalData);
    }

    public Task LogWarningAsync(string message, Dictionary<string, object>? additionalData = null)
    {
        return LogAsync(LogLevel.Warning, message, "Warning", null, additionalData);
    }

    public Task LogInformationAsync(string message, Dictionary<string, object>? additionalData = null)
    {
        return LogAsync(LogLevel.Information, message, "Info", null, additionalData);
    }

    public async Task<List<LogEntry>> GetLogsAsync(int count = 100)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _localLogs.OrderByDescending(l => l.Timestamp).Take(count).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ClearLogsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _localLogs.Clear();
            // También podrías limpiar el archivo local si estás persistiendo
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveLogLocallyAsync(LogEntry logEntry)
    {
        await _semaphore.WaitAsync();
        try
        {
            _localLogs.Add(logEntry);

            // Mantener solo los últimos 1000 logs en memoria
            if (_localLogs.Count > 1000)
            {
                _localLogs.RemoveRange(0, _localLogs.Count - 1000);
            }

            // Opcionalmente, guardar en archivo local
            await SaveToFileAsync(logEntry);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveToFileAsync(LogEntry logEntry)
    {
        try
        {
            var fileName = $"logs_{DateTime.UtcNow:yyyy-MM-dd}.json";
            var filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

            var json = JsonSerializer.Serialize(logEntry);
            await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
        }
        catch
        {
            // Si falla el guardado en archivo, no queremos que falle toda la aplicación
        }
    }

    private async Task SendLogToApiAsync(LogEntry logEntry)
    {
        try
        {
            // Simular latencia de red
            await Task.Delay(100);

            // Simular envío a API
            var json = JsonSerializer.Serialize(logEntry);

            // En producción, aquí harías algo como:
            // await _httpClient.PostAsJsonAsync("api/logs", logEntry);

            // Por ahora, simulamos éxito/fallo aleatorio
            var random = new Random();
            if (random.Next(10) > 8) // 20% de fallo
            {
                throw new Exception("Error simulado al enviar log a API");
            }

            // Log de éxito simulado
            Console.WriteLine($"[API LOG] Successfully sent: {logEntry.Level} - {logEntry.Message}");
        }
        catch (Exception ex)
        {
            // Si falla el envío a API, lo guardamos para reintentar más tarde
            await SaveFailedApiLogAsync(logEntry);
            Console.WriteLine($"[API LOG ERROR] Failed to send log: {ex.Message}");
        }
    }

    private async Task SaveFailedApiLogAsync(LogEntry logEntry)
    {
        try
        {
            var fileName = "failed_api_logs.json";
            var filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

            var json = JsonSerializer.Serialize(logEntry);
            await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
        }
        catch
        {
            // Ignorar si falla
        }
    }

    private void LogToConsole(LogEntry logEntry, Exception? exception)
    {
        var logLevel = logEntry.Level switch
        {
            LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };

        if (exception != null)
        {
            _logger.Log(logLevel, exception, logEntry.Message);
        }
        else
        {
            _logger.Log(logLevel, logEntry.Message);
        }
    }

    private string GetDeviceInfo()
    {
        try
        {
            return $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString} - {DeviceInfo.Current.Model}";
        }
        catch
        {
            return "Unknown Device";
        }
    }

    private string GetAppVersion()
    {
        try
        {
            return AppInfo.Current.VersionString;
        }
        catch
        {
            return "1.0.0";
        }
    }
}
