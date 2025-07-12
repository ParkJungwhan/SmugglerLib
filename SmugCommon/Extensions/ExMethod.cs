namespace SmugCommon.Extensions
{
    public static class ExtentionMethod
    {
        public static DateTime ToReturnTimeMinute(this DateTime time)
        {
            DateTime newTime;
            DateTime.TryParse(time.ToString("yyyy-MM-dd HH:mm:00"), out newTime);
            return newTime;
        }

        public static string ToAllString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        public static string ToDayString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd");
        }

        public static string ToTimeString(this DateTime time)
        {
            return time.ToString("HH:mm:ss.fff");
        }

        /// <summary>
        /// yyyy-MM-dd 00:00:00
        /// </summary>
        public static string ToDayBeginString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd 00:00:00");
        }

        /// <summary>
        /// yyyy-MM-dd 23:59:59
        /// </summary>
        public static string ToDayEndString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd 23:59:59");
        }

        /// <summary>
        /// yyyy-MM-dd HH:00:00
        /// </summary>
        public static string ToHourBeginString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:00:00");
        }

        /// <summary>
        /// yyyy-MM-dd HH:59:59
        /// </summary>
        public static string ToHourEndString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:59:59");
        }

        /// <summary>
        /// yyyy-MM-01 00:00:00
        /// </summary>
        public static string ToMonthBeginString(this DateTime time)
        {
            return time.ToString("yyyy-MM-01 00:00:00");
        }

        /// <summary>
        /// yyyy-MM-99 23:59:59
        /// </summary>
        public static string ToMonthEndString(this DateTime time)
        {
            return string.Format("{0}-{1} 23:59:59", time.ToString("yyyy-MM"), DateTime.DaysInMonth(time.Year, time.Month));
        }

        public static IEnumerable<TResult> FullJoinDistinct<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> leftItems,
            IEnumerable<TRight> rightItems,
            Func<TLeft, TKey> leftKeySelector,
            Func<TRight, TKey> rightKeySelector,
            Func<TLeft, TRight, TResult> resultSelector)
        {
            var leftJoin = from left in leftItems
                           join right in rightItems on leftKeySelector(left) equals rightKeySelector(right) into temp
                           from right in temp.DefaultIfEmpty()
                           select resultSelector(left, right);

            var rightJoin = from right in rightItems
                            join left in leftItems on rightKeySelector(right) equals leftKeySelector(left) into temp
                            from left in temp.DefaultIfEmpty()
                            select resultSelector(left, right);

            return leftJoin.Union(rightJoin);
        }
    }
}