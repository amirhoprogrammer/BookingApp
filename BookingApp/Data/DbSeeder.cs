namespace BookingApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (db.Courts.Any()) return;

        var courts = new List<Court>
        {
            new() { Name = "Football Court A", Description = "Outdoor 5-a-side, artificial grass", PricePerHour = 40 },
            new() { Name = "Basketball Court", Description = "Indoor, wooden floor", PricePerHour = 30 },
            new() { Name = "Tennis Court 1", Description = "Clay court with lighting", PricePerHour = 25 }
        };
        db.Courts.AddRange(courts);
        await db.SaveChangesAsync();

        foreach (var court in courts)
        {
            for (int day = 0; day < 7; day++)
            {
                for (int hour = 8; hour < 22; hour++)
                {
                    var start = DateTime.Today.AddDays(day).AddHours(hour);
                    db.TimeSlots.Add(new TimeSlot
                    {
                        CourtId = court.Id,
                        StartTime = start,
                        EndTime = start.AddHours(1)
                    });
                }
            }
        }
        await db.SaveChangesAsync();
    }
}