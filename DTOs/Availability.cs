namespace AllSet.DTOs
{
    public class AvailabilityResponseDto
    {
        public DateRepresentator Date { get; set; }
        public List<TimeSlotDto> AvailableTimeSlots { get; set; }

        public AvailabilityResponseDto(DateTime date)
        {
            Date = new DateRepresentator(date);
            AvailableTimeSlots = new List<TimeSlotDto>();
        }
    }

    public class TimeSlotDto
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class DateRepresentator
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public DateRepresentator() { }

        public DateRepresentator(DateTime date)
        {
            Year = date.Year;
            Month = date.Month;
            Day = date.Day;
        }

        public DateTime ToDateTime() => new DateTime(Year, Month, Day);
    }

    public class BookedSlotDto
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }

}

