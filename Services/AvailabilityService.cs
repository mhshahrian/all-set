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
            var dayOfWeek = date.DayOfWeek;

            var workingHours = await _dbContext.WorkingHours
                .Where(w => w.ResourceId == resourceId && w.DayOfWeek == dayOfWeek)
                .ToListAsync();

            if (!workingHours.Any()) return false;

            var startTime = startDateTime.ToUniversalTime().TimeOfDay;
            var endTime = endDateTime.ToUniversalTime().TimeOfDay;

            return workingHours.Any(w =>
                w.OpenTime <= startTime &&
                w.CloseTime >= endTime
            );
        }

        private async Task<AvailabilityResponseDto> GetDailyAvailability(Guid resourceId, DateTime date)
        {
            var workingHours = await _dbContext.WorkingHours
                .Where(w => w.ResourceId == resourceId && w.DayOfWeek == date.DayOfWeek).ToListAsync();

            /* Commenting out exception handling for this phase
            var exception = await _dbContext.ResourceExceptions
                .FirstOrDefaultAsync(e => e.ResourceId == resourceId && e.Date == date);
            */

            var bookedSlots = await _dbContext.Bookings
                .Where(b => !b.IsDeleted && b.ResourceId == resourceId &&
                b.StartDateTime.Date == date.ToUniversalTime().Date &&
                b.Status == "confirmed")
                .Select(b => new BookedSlotDto
                {
                    StartDateTime = b.StartDateTime.ToUniversalTime(),
                    EndDateTime = b.EndDateTime.ToUniversalTime()
                })
                .ToListAsync();


            var availability = new AvailabilityResponseDto(date.ToUniversalTime());

            if (/*exception?.IsClosed == true ||*/ !workingHours.Any())
            {
                return availability; // No available slots
            }

            foreach (var workingHour in workingHours.OrderBy(x => x.OpenTime))
            {
                var openTime = /*exception?.OpenTime ??*/ workingHour.OpenTime;
                var closeTime = /*exception?.CloseTime ??*/ workingHour.CloseTime;

                availability.AvailableTimeSlots.AddRange(CalculateFreeSlots(openTime, closeTime, bookedSlots));
            }

            return availability;
        }

        private List<TimeSlotDto> CalculateFreeSlots(TimeSpan open, TimeSpan close, List<BookedSlotDto> bookedSlots)
        {
            var availableSlots = new List<TimeSlotDto>();
            var currentTime = open;

            foreach (var booking in bookedSlots.OrderBy(b => b.StartDateTime))
            {
                if (currentTime < booking.StartDateTime.TimeOfDay)
                {
                    availableSlots.Add(new TimeSlotDto { Start = currentTime, End = booking.StartDateTime.TimeOfDay });
                }
                currentTime = booking.EndDateTime.TimeOfDay;
            }

            if (currentTime < close)
            {
                availableSlots.Add(new TimeSlotDto { Start = currentTime, End = close });
            }

            return availableSlots;
        }
    }
}
