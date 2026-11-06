using Microsoft.AspNetCore.Identity;

namespace BookingApp.Data;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

