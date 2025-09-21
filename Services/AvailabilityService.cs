using AllSet.Domain;
using AllSet.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AllSet.Services
{
    public class AvailabilityService
    {
        private readonly AllSetDbContext _dbContext;

        public AvailabilityService(AllSetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AvailabilityResponseDto>> GetAvailability(Guid resourceId, DateTime startDate, DateTime endDate)
        {
            var availabilityList = new List<AvailabilityResponseDto>();

            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var availability = await GetDailyAvailability(resourceId, date);
                availabilityList.Add(availability);
            }

            return availabilityList;
        }

        public async Task<bool> IsWithinWorkingHours(Guid resourceId, DateTime startDateTime, DateTime endDateTime)
        {
            var date = startDateTime.ToUniversalTime().Date;
            var (open, close) = await GetEffectiveWorkingHours(resourceId, date);

            if (open == null || close == null) return false;

            var startTime = startDateTime.ToUniversalTime().TimeOfDay;
            var endTime = endDateTime.ToUniversalTime().TimeOfDay;

            return open <= startTime && close >= endTime;
        }

        private async Task<AvailabilityResponseDto> GetDailyAvailability(Guid resourceId, DateTime date)
        {
            var resource = await _dbContext.Resources.FindAsync(resourceId);
            if (resource == null) return new AvailabilityResponseDto(date.ToUniversalTime());

            var bookedSlots = await _dbContext.Bookings
                .Where(b => !b.IsDeleted && b.ResourceId == resourceId &&
                            b.StartDateTime.Date == date.Date && b.Status == "confirmed")
                .Select(b => new BookedSlotDto
                {
                    StartDateTime = b.StartDateTime.ToUniversalTime(),
                    EndDateTime = b.EndDateTime.ToUniversalTime()
                }).ToListAsync();

            var availability = new AvailabilityResponseDto(date.ToUniversalTime());

            var (open, close) = await GetEffectiveWorkingHours(resourceId, date);
            if (open == null || close == null)
                return availability; // closed or holiday

            availability.AvailableTimeSlots.AddRange(CalculateFreeSlots(open.Value, close.Value, bookedSlots, resource.GapInMinutes));

            return availability;
        }

        private async Task<(TimeSpan? Open, TimeSpan? Close)> GetEffectiveWorkingHours(Guid resourceId, DateTime date)
        {
            var overrideEntry = await _dbContext.WorkingTimeOverrides
                .FirstOrDefaultAsync(w => w.ResourceId == resourceId && w.Date == date.Date);

            if (overrideEntry != null)
            {
                if (overrideEntry.IsClosed)
                    return (null, null);

                return (overrideEntry.OpenTime, overrideEntry.CloseTime);
            }

            var dayOfWeek = date.DayOfWeek;
            var workingHours = await _dbContext.WorkingHours
                .Where(w => w.ResourceId == resourceId && w.DayOfWeek == dayOfWeek)
                .OrderBy(w => w.OpenTime)
                .ToListAsync();

            if (!workingHours.Any())
                return (null, null);

            var open = workingHours.Min(w => w.OpenTime);
            var close = workingHours.Max(w => w.CloseTime);

            return (open, close);
        }

        private List<TimeSlotDto> CalculateFreeSlots(TimeSpan open, TimeSpan close, List<BookedSlotDto> bookedSlots, int gapInMinutes)
        {
            var availableSlots = new List<TimeSlotDto>();
            var currentTime = open;
            var gap = TimeSpan.FromMinutes(gapInMinutes);

            // Filter only bookings that actually overlap with the working window
            var relevantBookings = bookedSlots
                .Where(b => b.EndDateTime.TimeOfDay > open && b.StartDateTime.TimeOfDay < close)
                .OrderBy(b => b.StartDateTime)
                .ToList();

            foreach (var booking in relevantBookings)
            {
                var bookingStart = booking.StartDateTime.TimeOfDay;
                var bookingEnd = booking.EndDateTime.TimeOfDay;

                // Apply gap: booking effectively starts earlier and ends later
                var effectiveBookingStart = bookingStart - gap;
                var effectiveBookingEnd = bookingEnd + gap;

                // Ensure effective times don't go outside working hours
                effectiveBookingStart = TimeSpan.FromTicks(Math.Max(effectiveBookingStart.Ticks, open.Ticks));
                effectiveBookingEnd = TimeSpan.FromTicks(Math.Min(effectiveBookingEnd.Ticks, close.Ticks));

                if (currentTime < effectiveBookingStart)
                {
                    var slotEnd = effectiveBookingStart > close ? close : effectiveBookingStart;
                    availableSlots.Add(new TimeSlotDto { Start = currentTime, End = slotEnd });
                }

                // Move currentTime forward only if it's behind this booking (including gap)
                currentTime = TimeSpan.FromTicks(Math.Max(currentTime.Ticks, effectiveBookingEnd.Ticks));
            }

            // Add final slot if any time remains
            if (currentTime < close)
            {
                availableSlots.Add(new TimeSlotDto { Start = currentTime, End = close });
            }

            return availableSlots;
        }

    }
}
