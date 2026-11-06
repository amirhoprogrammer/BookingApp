using BookingApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Services;

public record BookingResult(bool Success, string? Error = null);

public class BookingService(IDbContextFactory<ApplicationDbContext> factory)
{
    public async Task<List<Court>> GetCourtsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Courts.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<List<TimeSlot>> GetAvailableSlotsAsync(int courtId, DateTime date)
    {
        await using var db = await factory.CreateDbContextAsync();
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        return await db.TimeSlots
            .AsNoTracking()
            .Where(s => s.CourtId == courtId
                        && s.StartTime >= dayStart && s.StartTime < dayEnd
                        && s.StartTime > DateTime.Now
                        && !s.Bookings.Any(b => b.Status == BookingStatus.Confirmed))
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<BookingResult> BookAsync(string userId, int timeSlotId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var slot = await db.TimeSlots.FindAsync(timeSlotId);
        if (slot is null)
            return new(false, "Time slot not found.");
        if (slot.StartTime <= DateTime.Now)
            return new(false, "You cannot book a time in the past.");

        db.Bookings.Add(new Booking { UserId = userId, TimeSlotId = timeSlotId });

        try
        {
            await db.SaveChangesAsync();
            return new(true);
        }
        catch (DbUpdateException)
        {
            // unique index violation: someone else booked this slot first
            return new(false, "This time slot has just been booked by someone else.");
        }
    }

    public async Task<List<Booking>> GetUserBookingsAsync(string userId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.TimeSlot).ThenInclude(s => s.Court)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.TimeSlot.StartTime)
            .ToListAsync();
    }

    public async Task<BookingResult> CancelAsync(string userId, int bookingId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var booking = await db.Bookings
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

        if (booking is null)
            return new(false, "Booking not found.");
        if (booking.Status == BookingStatus.Cancelled)
            return new(false, "This booking is already cancelled.");
        if (booking.TimeSlot.StartTime - DateTime.Now < TimeSpan.FromHours(24))
            return new(false, "Bookings can only be cancelled at least 24 hours in advance.");

        booking.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync();
        return new(true);
    }
}