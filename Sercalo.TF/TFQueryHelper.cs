using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sercalo.Serial.Query;
using CI = System.Globalization.CultureInfo;

namespace Sercalo.TF.Query
{
    public static class TFQueryHelper
    {
        #region CONSTANTS

        const string REGEX_DEFAULT_VALUE = "val";
        private static char[] trimChars = new char[] { '\r', '\n', '\0' };

        #endregion

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Gets the value from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        public static async Task<string> GetStringValue(this ITunableFilter device, string function)
            => await QueryAndMatchAsync(device, function, $"^{function} (?<{REGEX_DEFAULT_VALUE}>.+)$");

        /// <summary>
        /// Sets the value from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static async Task<bool> SetStringValue(this ITunableFilter device, string function, string value)
        {
            return value == await QueryAndMatchAsync(device, $"{function} {value}", $"^{function} (?<{REGEX_DEFAULT_VALUE}>.+)$");
        }

        /// <summary>
        /// Gets the value in byte from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        public static async Task<Point> GetXY(this ITunableFilter device, string function)
            => StringToPoint(await GetStringValue(device, function));

        /// <summary>
        /// Sets the value in byte from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static async Task<bool> SetXY(this ITunableFilter device, string function, Point value)
            => await SetStringValue(device, function, PointToString(value));

        /// <summary>
        /// Gets the value in byte from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        public static async Task<T> GetValue<T>(this ITunableFilter device, string function) where T : IConvertible
            => (T)Convert.ChangeType(await GetStringValue(device, function), typeof(T), CI.InvariantCulture);

        /// <summary>
        /// Sets the value in byte from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static async Task<bool> SetValue<T>(this ITunableFilter device, string function, T value) where T : IConvertible
        {
            string str;

            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Decimal: 
                case TypeCode.Single:
                case TypeCode.Double: str = value.ToDouble(CI.InvariantCulture).ToString("F3", CI.InvariantCulture); break;
                default: str = (string)Convert.ChangeType(value, TypeCode.String, CI.InvariantCulture); break;
            }

            return await SetStringValue(device, function, str);
        }

        /// <summary>
        /// Approves the result or throw an exception summarizing the error
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="result">The result.</param>
        /// <exception cref="Sercalo.TF.TunableFilterException">
        /// <returns></returns>
        public static bool ApproveResult(this ITunableFilter device, string result)
        {
            ThrowIfError(result);
            return true;
        }

        #endregion

        #region PRIVATE FUNCTIONS

        /// <summary>
        /// Throws if error.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <exception cref="Sercalo.TF.TunableFilterException">
        /// </exception>
        private static void ThrowIfError(string result)
        {
            Match match = Regex.Match(result, @"^ERR (?>(?<n>\d+)|(?<t>.+))$");

            if (match.Success)
            {
                if (match.Groups["n"].Success)
                    throw new TunableFilterException((ErrorCode)byte.Parse(match.Groups["n"].Value));
                else
                    throw new TunableFilterException(match.Groups["t"].Value);
            }
        }

        /// <summary>
        /// Send the specified input and wait for an output asynchronously, then match the string with specified pattern that include the val value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        private static async Task<string> QueryAndMatchAsync(this ITunableFilter device, string input, string pattern)
        {
            string output = (await device.QueryAsync(input)).TrimEnd(trimChars);

            Match match = Regex.Match(output, pattern);

            if (!match.Success)
            {
                ThrowIfError(output);

                throw new SercaloException($"Cannot find a suitable match from expresion '{output}'.");
            }

            return match.Groups[REGEX_DEFAULT_VALUE].Value;
        }

        /// <summary>
        /// Convert a String to point.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        private static Point StringToPoint(string str)
        {
            string[] strs = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (strs.Length != 4)
                throw new FormatException(str);

            return new Point(GetValueFromElectrodes(strs[0], strs[1]), GetValueFromElectrodes(strs[2], strs[3]));
        }

        /// <summary>
        /// Convert a Point to string.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        private static string PointToString(Point point)
        {
            (int x_neg, int x_pos) = GetElectrodeValues(point.X);
            (int y_neg, int y_pos) = GetElectrodeValues(point.Y);

            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1} {2} {3}", x_neg, x_pos, y_neg, y_pos);
        }

        /// <summary>
        /// Gets the electrode values from the int vlaue
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static (int neg, int pos) GetElectrodeValues(int value)
        {
            int neg = 0;
            int pos = 0;

            if (value >= 0) pos = value;
            else neg = -value;

            return (neg, pos);
        }

        /// <summary>
        /// Gets the value from electrodes.
        /// </summary>
        /// <param name="neg">The neg.</param>
        /// <param name="pos">The position.</param>
        /// <returns></returns>
        private static int GetValueFromElectrodes(string neg, string pos)
        {
            int value;

            if (int.TryParse(neg, out value)
                && value > 0)
                return -value;

            value = int.Parse(pos, System.Globalization.CultureInfo.InvariantCulture);

            return value;
        }

        #endregion
    }
}
