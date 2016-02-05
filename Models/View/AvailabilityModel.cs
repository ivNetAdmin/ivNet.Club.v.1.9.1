
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ivNet.Club.Models.View
{
    public class AvailabilityModel
    {
        public AvailabilityModel()
        {
            Months = new List<Month>();
            var months = new[] {"Apr", "May", "Jun", "Jul", "Aug", "Sep"};
            var firstofMonth = new DateTime(DateTime.Now.Year, 4, 1);
            var firstSaturday = firstofMonth.AddDays(-(firstofMonth.DayOfWeek - DayOfWeek.Saturday));
            var sunday = firstSaturday.AddDays(1);

            var sundayFirstOfMonth = false;
            while (sunday < new DateTime(DateTime.Now.Year, 10, 1))
            {
                var month = MonthExists(months[firstSaturday.Month - 4]);

                if(month==null)
                {
                    month = new Month {Name = months[firstSaturday.Month - 4]};
                    Months.Add(month);
                }

                if (sundayFirstOfMonth && month.Days.Count == 0)
                {
                    month.Days.Add(new Day {Date = "01",Weekend = "Sun"});
                    sundayFirstOfMonth = false;
                }

                month.Days.Add(new Day{Date=firstSaturday.Day.ToString(CultureInfo.InvariantCulture).PadLeft(2,'0'),Weekend = "Sat"});
                if (!sundayFirstOfMonth) month.Days.Add(new Day { Date = sunday.Day.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), Weekend = "Sun"});

                firstSaturday = firstSaturday.AddDays(7);
                sunday = firstSaturday.AddDays(1);

                if (sunday.Day == 1) sundayFirstOfMonth = true;

            }        

        }
             
        public List<Month> Months { get; set; }
        public int PlayerId { get; set; }

        public class Month
        {
            public Month()
            {
                Days=new List<Day>();
            }
            public string Name { get; set; }
            public List<Day> Days { get; set; } 
        }

        public class Day
        {
            public string Date { get; set; }
            public bool Available { get; set; }
            public string Weekend { get; set; }
        }

        private Month MonthExists(string monthName)
        {
            return Months.FirstOrDefault(month => month.Name == monthName);
        }
    }
}