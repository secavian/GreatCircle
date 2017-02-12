using System;

namespace GreatCIrcle.Domain.Extensions
{
    /// <summary>
    /// Numerics extension methods
    /// </summary>
    /// <remarks>
    /// The methods in this class provide aesthetic short cuts to common actions or
    /// allow double => text conversions to accommodate NaN values.
    /// </remarks>
    public static class NumericsEx
    {
        /// <summary>
        /// Shortcut for double.IsNaN(value)
        /// </summary>
        /// <param name="value">Double value being checked</param>
        /// <returns><c>true</c> if value is Nan; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// This is strictly an aesthetic. I prefer if(d.IsNaN()) {...} to if(double.IsNaN(d)) {...}
        /// </remarks>
        public static bool IsNaN(this double value) => double.IsNaN(value);

        /// <summary>
        /// Shortcut for !double.IsNaN(value)
        /// </summary>
        /// <param name="value">Double value being checked</param>
        /// <returns><c>true</c> if value is not NaN; otherwise <c>false</c></returns>
        /// <remarks>
        /// This is strictly an aesthetic. I prefer if(d.NotIsNaN()) {...} to if(!double.IsNaN(d)) {...}
        /// </remarks>
        public static bool NotIsNaN(this double value) => !double.IsNaN(value);

        /// <summary>
        /// Cheater method. Gets double value as string. If NaN, returns "". Does not round.
        /// </summary>
        /// <param name="value">Double value to convert to string</param>
        /// <returns>String representation of value. No rounding.</returns>
        /// <remarks>See remarks for overloaded method.</remarks>
        public static string Text(this double value) => Text(value, -1);

        /// <summary>
        /// Cheater method. Gets double value as string. If NaN, returns ""
        /// </summary>
        /// <param name="value">Double value to convert to string</param>
        /// <param name="round">Rounding digits. Use -1 to turn rounding off
        /// (better yet, use the overloaded method).</param>
        /// <returns>String representation of value.</returns>
        /// <remarks>
        /// This was created because of statements like this:
        /// 
        ///     double d = double.NaN;
        ///     string test = "value: " + d.ToString();
        ///     
        /// This results in "Value: NaN". Usually, for text purposes, an NaN would be better as "", not "NaN".
        /// 
        /// This cheater method prevents the need for something like this:
        /// 
        ///     string test = "value: " + (double.IsNaN(d) ? "" : d.ToString());
        ///     
        /// Rounding was added because, why not?
        /// </remarks>
        public static string Text(this double value, int round)
        {
            if (value.IsNaN()) { return ""; }
            if (round > -1) { value = Math.Round(value, round); }
            return value.ToString();
        }
    }
}
