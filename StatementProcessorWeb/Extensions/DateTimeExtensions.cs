namespace StatementProcessorWeb.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime PreviousBusinessDay(this DateTime dt)
        {
            return dt.DayOfWeek switch
            {
                DayOfWeek.Sunday => dt.AddDays(-2),
                DayOfWeek.Monday => dt.AddDays(-3),
                DayOfWeek.Saturday or DayOfWeek.Friday or DayOfWeek.Thursday or DayOfWeek.Wednesday or DayOfWeek.Tuesday
                    => dt.AddDays(-1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

    }
}
