namespace LalaHealthCare.DataAccess.Models;

public class CheckOutDto
{
    public string ScheduleId { get; set; }
    public DateTime CheckOutTime { get; set; }

    public decimal CheckOutLatitude { get; set; }

    public decimal CheckOutLongitude { get; set; }

    public string CheckOutAddress { get; set; }

    public string Observations { get; set; }

    public string SignatureData { get; set; }
}
