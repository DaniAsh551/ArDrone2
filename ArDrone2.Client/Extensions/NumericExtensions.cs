using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ArDrone2.Client
{
    public static class NumericExtensions
    {
        public static int Normalize(this float value)
        {
            int resultingValue = 0;
            unsafe
            {
                value = (Math.Abs(value) > 1) ? 1 : value;
                resultingValue = *(int*)(&value);
            }

            return resultingValue;
        }

        /// <summary>
        /// Returns whether the Difference between two doubles is tolerable.
        /// </summary>
        /// <param name="d1">Double one.</param>
        /// <param name="d2">Double two.</param>
        /// <param name="tolerance">The amount of difference tolerable between the two numbers.</param>
        /// <returns>Whether the Difference between two doubles is tolerable.</returns>
        public static bool IsTolerable(this double d1, double d2, double tolerance)
        {
            if (Math.Abs(d1 - d2) <= tolerance)
                return true;
            return (Math.Abs(d2 - d1) <= tolerance);
        }

        public static double Average(this double dbl, params double[] doubles)
            => (doubles.Sum() + dbl) / (doubles.Length + 1);
    }
}