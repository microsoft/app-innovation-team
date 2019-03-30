using System;

namespace BotApp
{
    public class DateTimeHelper
    {
        public static DateTime GetCustomTimeZone()
        {
            TimeZoneInfo setTimeZoneInfo;
            DateTime currentDateTime;

            if (string.IsNullOrEmpty(Settings.TimeZone))
                throw new Exception("TimeZone is required");

            //Set the time zone information to US Mountain Standard Time
            setTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Settings.TimeZone);

            //Get date and time in US Mountain Standard Time
            currentDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, setTimeZoneInfo);
            return currentDateTime;//.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}