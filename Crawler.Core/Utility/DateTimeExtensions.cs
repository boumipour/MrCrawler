using System;
using System.Globalization;

namespace Utility.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly PersianCalendar _persianCalendar = new();
        public static string ToPersianDate(DateTime dateTime)
        {
            return string.Format("{0}/{1}/{2}", _persianCalendar.GetYear(dateTime), _persianCalendar.GetMonth(dateTime), _persianCalendar.GetDayOfMonth(dateTime));
        }

        public static string ToPersianDate(this string dateTimeString)
        {
            DateTime dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return ToPersianDate(dateTime);
        }
    }
}
