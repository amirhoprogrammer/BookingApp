namespace BookingApp.Data;

public class TimeSlot
{
    public int Id { get; set; }
    public int CourtId { get; set; }
    public Court Court { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}