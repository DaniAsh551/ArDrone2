using System;

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
    }
}