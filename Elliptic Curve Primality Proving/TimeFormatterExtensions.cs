using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elliptic_Curve_Primality_Proving
{
    internal static class TimeFormatterExtensions
    {
        /* Formats a given time component with leading zeros. */
        private static void AppendWithLeadingZeros(StringBuilder sb, int value, int digits)
        {
            sb.Append(value.ToString().PadLeft(digits, '0'));
        }

        /// <summary>
        /// This function formats a given full timestamp component.
        /// </summary>
        public static string FormatTime(this TimeSpan ts)
        {
            var sb = new StringBuilder();

            if (ts.Days > 0)
            {
                AppendWithLeadingZeros(sb, ts.Days, 2);
                sb.Append('.');
            }

            AppendWithLeadingZeros(sb, ts.Hours, 2); sb.Append(':');
            AppendWithLeadingZeros(sb, ts.Minutes, 2); sb.Append(':');

            AppendWithLeadingZeros(sb, ts.Seconds, 2); sb.Append('.');
            AppendWithLeadingZeros(sb, ts.Milliseconds, 3);
            return sb.ToString();
        }
    }
}
