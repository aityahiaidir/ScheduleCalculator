
using System.Globalization;

namespace akerdis.Scheduler

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



        public bool isWorkTime(TimeSpan time)

        {
            TimeSpan timeCore = default(TimeSpan).Add(time);
            return (timeCore >= FromTime) && (timeCore <= ToTime);

        }



    }

    public class WorkDay

    {

        public DayOfWeek Day { get; set; }
        public bool isNotWorkDay { get; set; } = false;
        public IList<WorkTime>? WorkTimes { get; set; }

        public WorkDay(DayOfWeek day, bool notWorkDay = false)

        {

            Day = day;

            isNotWorkDay = notWorkDay;

            if (notWorkDay == false)

            {
                WorkTimes = new List<WorkTime>

                {
                    new WorkTime(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)),
                    new WorkTime(new TimeSpan(13, 0, 0), new TimeSpan(16, 0, 0))
                };

            }

            

        }

        public bool isWorkingDay { get { return !isNotWorkDay; }  }
      

    }





    public class Holiday

    {



        public DateTime HolidayDate { get; set; }
        public string? HolidayName { get; set; }
        public int NumberOfDays { get; set; } = 1;



        public Holiday(DateTime date)

        {
            this.HolidayDate = date;
        }

        public Holiday(DateTime date, string name)

        {
            this.HolidayDate = date;
            this.HolidayName = name;

        }



        public Holiday(DateTime date, string name, int number)

        {
            this.HolidayDate = date;
            this.HolidayName = name;
            this.NumberOfDays = number;
        }


    }


    /// <summary>
    /// Class <c>ScheduleCalculator</c> utility class that allows us calculating the finish date and time 
    /// of a working task.
    /// /// <para>
    /// the calculation is based on the start date and  duration and takes into account
    /// of public holidays, business working days 
    /// </para>
    /// </summary>
    /// <author>
    /// AIT YAHIA Idir
    /// </author>
    /// <date>
    /// 02/27/2023
    /// </date>
    internal class ScheduleCalculator

    {

        protected Dictionary<DayOfWeek, WorkDay> WorkDayOfWeeks = new Dictionary<DayOfWeek, WorkDay>()

        {
            { DayOfWeek.Sunday, new WorkDay(DayOfWeek.Sunday) },
            { DayOfWeek.Monday, new WorkDay(DayOfWeek.Monday) },
            { DayOfWeek.Tuesday, new WorkDay(DayOfWeek.Tuesday) },
            { DayOfWeek.Wednesday, new WorkDay(DayOfWeek.Wednesday)},
            { DayOfWeek.Thursday, new WorkDay(DayOfWeek.Thursday) },
            { DayOfWeek.Friday, new WorkDay(DayOfWeek.Friday,true) },
            { DayOfWeek.Saturday, new WorkDay(DayOfWeek.Saturday,true) },
         };



        public Holiday[] Holidays { get; set; } = { new Holiday(new(2023, 1, 1), "New year"),new Holiday(new DateTime(2023,2,26),"Jour de test",2) };

        public bool isHoliday(DateTime date)
        {
            return Array.Exists(Holidays, holiday => {
                if ((date >= holiday.HolidayDate) && (date <= holiday.HolidayDate.AddDays(holiday.NumberOfDays-1)))
                    { return true; }
                return false;
            });
        }

        public static bool IsValidTimeFormat(string input)
        {
            TimeSpan dummyOutput;
            return TimeSpan.TryParse(input, out dummyOutput);
        }

        public static bool IsValidDateTimeFormat(string input)
        {
            DateTime dummyOutput;
            return DateTime.TryParse(input, out dummyOutput);
        }

        public DateTime CalcFinishDate(string startDate,string duration)

        {

    
            DateTime _startDate = DateTime.ParseExact(startDate, "dd/MM/yyyy HH:mm",
                                       CultureInfo.InvariantCulture);
            TimeSpan _duration = default(TimeSpan).Add(TimeSpan.ParseExact(duration, "hh\\:mm", CultureInfo.InvariantCulture)); ;
            return CalcFinishDate(_startDate, _duration);
        }
        public DateTime CalcFinishDate(DateTime startDate, TimeSpan duration)

        {
            if (duration == TimeSpan.Zero) { return startDate; }



           TimeSpan remainingDuration = duration;
           DateTime start_date = startDate;
           DateTime end_date = startDate + remainingDuration;
           DateTime nextDay = startDate.Date;
           TimeSpan timeToReduce= TimeSpan.Zero;



            while (remainingDuration > TimeSpan.Zero)

            {
                DayOfWeek day = nextDay.DayOfWeek;
                WorkDay wd = WorkDayOfWeeks[day];

                if ((wd.isWorkingDay) && (!isHoliday(nextDay)))

                {

                    foreach (WorkTime wt in wd.WorkTimes)

                    {
                        if (remainingDuration > TimeSpan.Zero)
                        {
                            DateTime period_start_date = nextDay.Date.Add(wt.FromTime);
                            DateTime period_end_date = nextDay.Date.Add(wt.ToTime);

                            if (start_date <= period_start_date)
                            {
                               
                                start_date = period_start_date;
                                end_date = start_date.Add(remainingDuration);

                                if (end_date < period_start_date)
                                {
                                    timeToReduce = TimeSpan.Zero;
                                }
                                else
                                if ((end_date >= period_start_date) && (end_date < period_end_date))
                                {
                                    timeToReduce = end_date.Subtract(period_start_date);

                                }
                                else
                                if (end_date > period_end_date)
                                {
                                    end_date = period_end_date;
                                    timeToReduce = end_date.Subtract(start_date);

                                }
                            }

                            else if (start_date > period_start_date)
                            {
                                if (start_date > period_end_date)
                                {
                                    timeToReduce = TimeSpan.Zero;
                                }
                                else
                                    if ((start_date < end_date) && (end_date > period_end_date))
                                {
                                    end_date = period_end_date;
                                    timeToReduce = end_date.Subtract(start_date);

                                }
                                else if (end_date < period_end_date)
                                {
                                    timeToReduce = end_date.Subtract(start_date);

                                }
                            }


                            remainingDuration = remainingDuration.Subtract(timeToReduce);
                          
                        }
                    }
                }

                nextDay = nextDay.AddDays(1);

             }

            Console.WriteLine("* end date :" + end_date.ToString());

            return end_date;

        }







    }





}





