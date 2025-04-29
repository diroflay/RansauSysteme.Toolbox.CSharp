namespace RansauSysteme.Utils
{
    public static class NumberUtils
    {
        /// <summary>
        /// Determine if two float values are equals with a known epsilon tolerance
        /// By default epsilon has the value 1E-10
        /// </summary>
        /// <param name="value">The float value</param>
        /// <param name="x">The value to compare</param>
        /// <param name="epsilon">The tolerance</param>
        /// <returns>True if nearly equals, else false</returns>
        public static bool NearlyEquals(this float value, float x, float? epsilon = null)
        {
            float tolerance = epsilon ?? Math.Max(Math.Abs(value), Math.Abs(x)) * 1E-10f;
            return Math.Abs(value - x) <= tolerance;
        }

        /// <summary>
        /// Determine if two double values are equals with a known epsilon tolerance
        /// By default epsilon has the value 1E-10
        /// </summary>
        /// <param name="value">The double value</param>
        /// <param name="x">The value to compare</param>
        /// <param name="epsilon">The tolerance</param>
        /// <returns>True if nearly equals, else false</returns>
        public static bool NearlyEquals(this double value, double x, double? epsilon = null)
        {
            double tolerance = epsilon ?? Math.Max(Math.Abs(value), Math.Abs(x)) * 1E-10;
            return Math.Abs(value - x) <= tolerance;
        }
    }
}