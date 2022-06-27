﻿using System;
using System.Collections.Generic;
using VErp.Commons.Constants;

namespace VErp.Commons.Library
{
    public static class NumberUtils
    {

        public static decimal RoundBy(this decimal value, int? decimalPlace = 11)
        {
            if (decimalPlace == null) decimalPlace = 11;
            return Math.Round(value, decimalPlace.Value, MidpointRounding.AwayFromZero);
        }
       

        public static decimal AddDecimal(this decimal a, decimal b)
        {
            if (a < 0 && b > 0 || a > 0 && b < 0)
            {
                var c = a + b;
                if (Math.Abs(c) < Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER) return 0;
                return c.RoundBy();
            }
            return (a + b).RoundBy();
        }

        public static decimal SubDecimal(this decimal a, decimal b)
        {
            if (a > 0 && b > 0 || a < 0 && b < 0)
            {
                var c = a - b;
                if (Math.Abs(c) < Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER) return 0;
                return c.RoundBy();
            }
            return (a - b).RoundBy();
        }

        public static decimal SubProductionDecimal(this decimal a, decimal b)
        {
            if (a > 0 && b > 0 || a < 0 && b < 0)
            {
                var c = a - b;
                if (Math.Abs(c) < Numbers.PRODUCTION_MINIMUM_ACCEPT_DECIMAL_NUMBER) return 0;
                return c;
            }
            return a - b;
        }
        public static decimal RelativeTo(this decimal value, decimal relValue)
        {
            if (Math.Abs(value) < Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER) return 0;
            if (value.SubDecimal(relValue) == 0) return relValue;
            return value;
        }


        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float),

            typeof(int?),  typeof(double?),  typeof(decimal?),
            typeof(long?), typeof(short?),   typeof(sbyte?),
            typeof(byte?), typeof(ulong?),   typeof(ushort?),
            typeof(uint?), typeof(float?)
        };
        public static bool IsNumber(this Type objectType)
        {
            return NumericTypes.Contains(objectType);
        }

    }
}
