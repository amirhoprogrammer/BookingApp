namespace BookingApp.Data;

public class Court
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PricePerHour { get; set; }
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}