using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace LalaHealthCare.DataAccess.Models;

public class Visit
{
    public string Id { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientProfilePictureUrl { get; set; } = string.Empty;
    public DateTime ScheduledDateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string StatusEchart { get; set; }   
    public VisitStatus Status { get; set; }
    public string NurseId { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Check-In
    public DateTime? CheckInTime { get; set; }
    public decimal? CheckInLatitude { get; set; }
    public decimal? CheckInLongitude { get; set; }
    public string? CheckInAddress { get; set; }

    // Check-Out
    public DateTime? CheckOutTime { get; set; }
    public decimal? CheckOutLatitude { get; set; }
    public decimal? CheckOutLongitude { get; set; }
    public string? CheckOutAddress { get; set; }
    public string? Observations { get; set; }
    public string? SignatureData { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VisitStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled
}
