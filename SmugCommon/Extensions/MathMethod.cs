namespace SmugCommon.Extensions
{
    internal class MathMethod
    {
        public static bool IsEmptyString(string x)
        {
            return string.IsNullOrEmpty(x);
        }

        public static bool IsPositive(int x)
        {
            return x > 0;
        }

        public static bool IsPositive(long x)
        {
            return x > 0;
        }

        public static bool IsPositive(float x)
        {
            return x > 0f;
        }

        public static bool IsPositive(double x)
        {
            return x > 0.0;
        }

        public static bool IsPositiveIncludeZero(int x)
        {
            return x >= 0;
        }

        public static bool IsPositiveIncludeZero(long x)
        {
            return x >= 0;
        }

        public static bool IsPositiveIncludeZero(float x)
        {
            return x >= 0f;
        }

        public static bool IsPositiveIncludeZero(double x)
        {
            return x >= 0.0;
        }

        public static bool IsNotZero(int x)
        {
            return x != 0;
        }

        public static bool IsNotZero(long x)
        {
            return x != 0;
        }

        public static bool IsNotZero(float x)
        {
            return x != 0f;
        }

        public static bool IsNotZero(double x)
        {
            return x != 0.0;
        }
    }
}