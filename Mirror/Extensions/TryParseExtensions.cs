using System;


namespace Mirror.Extensions
{
    static class TryParseExtensions
    {
        internal delegate bool ParseDelegate<T>(string value, out T result) where T : struct;

        internal static TEnum To<TEnum>(this string value)
            where TEnum : struct, IConvertible, IComparable, IFormattable =>
                TryParse<TEnum>(value, Enum.TryParse);
        
        internal static T TryParse<T>(this string value, ParseDelegate<T> parse) where T : struct
        {
            T result;
            parse(value as string, out result);
            return result;
        }
    }
}