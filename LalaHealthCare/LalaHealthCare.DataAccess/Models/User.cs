namespace LalaHealthCare.DataAccess.Models;

public class User
{
    public int Id { get; set; }
    public string ClinicianId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Nurse";
    public string? ProfilePictureUrl { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public DateTime LastLogin { get; set; }
}
