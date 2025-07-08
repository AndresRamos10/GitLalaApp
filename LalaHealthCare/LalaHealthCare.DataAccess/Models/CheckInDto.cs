namespace LalaHealthCare.DataAccess.Models
{
    public class CheckInDto
    {
        public string ScheduleId { get; set; }
        public DateTime CheckInTime { get; set; }

        public decimal CheckInLatitude { get; set; }

        public decimal CheckInLongitude { get; set; }

        public string CheckInAddress { get; set; }
    }
}
