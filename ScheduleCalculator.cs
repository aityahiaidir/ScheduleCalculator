using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Akerdis.Sheduler
{
    public class WorkTime
    {
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }

        public WorkTime(TimeSpan from, TimeSpan to)
        {
            FromTime = from;
            ToTime = to;
        }
    }

    public class WorkDay : IEnumerable<(DateTime start, DateTime end)>
    {
        public DateOnly Date { get; private set; }
        public IList<WorkTime> WorkTimes { get; private set; } = new List<WorkTime>();

        public WorkDay(DateOnly date, DayType dayType = DayType.Workday)
        {
            this.Date = date;
            if (dayType == DayType.Workday)
            {
                this.WorkTimes.Add(new WorkTime(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)));
                this.WorkTimes.Add(new WorkTime(new TimeSpan(13, 0, 0), new TimeSpan(16, 0, 0)));
            }
        }

        public bool IsWorkingDay => !this.WorkTimes.Any();

        public IEnumerator<(DateTime start, DateTime end)> GetEnumerator() =>
            this
                .WorkTimes
                .Select(x => (this.Date.ToDateTime(x.FromTime), this.Date.ToDateTime(x.ToTime)))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public class Holiday
    {
        public DateOnly Date { get; private set; }
        public string? Name { get; private set; }
        public int Days { get; private set; }

        public Holiday(DateOnly date) : this(date, "Unnamed") { }

        public Holiday(DateOnly date, string name) : this(date, name, 1) { }

        public Holiday(DateOnly date, string name, int days)
        {
            this.Date = date;
            this.Name = name;
            this.Days = days;
        }
    }

    public class ScheduleCalculator
    {
        private Holiday[] Holidays { get; set; } = {new(new(2023,1,1)), new(new(2023,2,26),"laid")};

        private bool IsHoliday(DateOnly date) =>
            date.DayOfWeek == DayOfWeek.Friday
            || date.DayOfWeek == DayOfWeek.Saturday
            || this.Holidays.Where(x => date >= x.Date && date < x.Date.AddDays(x.Days)).Any();

     
        private IEnumerable<WorkDay> WorkDayGenerator(DateOnly startDate)
        {
            DateOnly date = startDate;
            while (true)
            {
                yield return new WorkDay(date, this.IsHoliday(date) ? DayType.Nonworkday : DayType.Workday);
                date = date.AddDays(1);
            }
        }

        private IEnumerable<WorkDay> WorkDayGenerator(DateOnly startDate,DateOnly endDate)
        {
            DateOnly date = startDate;
            while (date <= endDate)
            {
                yield return new WorkDay(date, this.IsHoliday(date) ? DayType.Nonworkday : DayType.Workday);
                date = date.AddDays(1);
            }
        }
        public TimeSpan CalcDuration(DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime == endDateTime) return TimeSpan.Zero;
            DateTime currentDateTime = startDateTime;
            TimeSpan duration = TimeSpan.Zero;
            foreach (WorkDay workDay in WorkDayGenerator(startDateTime.ToDateOnly(),endDateTime.ToDateOnly()))
            {
                foreach ((DateTime workStartDateTime, DateTime workEndDateTime) in workDay)
                {
                    if (currentDateTime < workStartDateTime)
                    {
                        currentDateTime = workStartDateTime;
                    }
                     if (currentDateTime < workEndDateTime)
                       {
                        if (workEndDateTime >= endDateTime)
                        {
                            return duration.Add(endDateTime - currentDateTime);
                        }
                        
                        duration += (workEndDateTime - currentDateTime);

                      
                      }

                 }
            }

            return duration;

         }

        public void SetHolidays(Holiday[] holidays)
        {
            this.Holidays = holidays;

        }
        public DateTime CalcFinishDateTime(DateTime startDateTime, TimeSpan duration)
        {
            if (duration == TimeSpan.Zero) return startDateTime;
            DateTime currentDateTime = startDateTime;
            foreach (WorkDay workDay in WorkDayGenerator(currentDateTime.ToDateOnly()))
            {
                foreach ((DateTime workStartDateTime, DateTime workEndDateTime) in workDay)
                {
                    if (currentDateTime < workStartDateTime)
                    {
                        currentDateTime = workStartDateTime;
                    }

                    if (currentDateTime < workEndDateTime)
                    {
                        TimeSpan remaining = workEndDateTime - currentDateTime;
                        if (duration < remaining)
                        {
                            return currentDateTime.Add(duration);
                        }
                        if (duration == remaining)
                        {
                            return workEndDateTime;
                        }

                        duration -= remaining;
                    }
                }
            }
            //Code can never get here as WorkDayGenerator is infinite
            return default(DateTime);
        }
    }

    public enum DayType { Workday, Nonworkday, }

    public static class DateEx
    {
        public static DateOnly ToDateOnly(this DateTime dateTime) => new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);

        public static DateTime ToDateTime(this DateOnly dateOnly) => new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);

        public static DateTime ToDateTime(this DateOnly dateOnly, TimeSpan timeSpan) => dateOnly.ToDateTime().Add(timeSpan);

        public static TimeSpan? TryParseTimeFormat(this string input) => TimeSpan.TryParseExact(input, "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan timeSpan) ? timeSpan : null;

        public static DateTime? TryParseDateTimeFormat(string input) => DateTime.TryParseExact(input, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime) ? dateTime : null;
    }
}
