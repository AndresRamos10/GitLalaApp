namespace LalaHealthCare.DataAccess.Models;

public class Offer
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientImageUrl { get; set; } = string.Empty;
    public DateTime ScheduledDateTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty; // "Single Visit (fr)"
    public string LocationAddress { get; set; } = string.Empty;
    public string LocationPostalCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? DeclineReason { get; set; }
    public bool IsViewed { get; set; }
}

public enum OfferStatus
{
    Pending,
    Accepted,
    Declined,
    Expired
}
