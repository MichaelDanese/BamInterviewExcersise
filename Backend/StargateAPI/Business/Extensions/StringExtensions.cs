using System.Text.RegularExpressions;

namespace StargateAPI.Business.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Normalizes a string by trimming whitespace. 
        /// Also replaces multiple consecutive spaces with a single space. 
        /// </summary>
        /// <param name="input">The input string to normalize.</param>
        /// <returns>A normalized version of the input string to fit name and title standards.</returns>
        public static string NormalizeNameOrTitle(this string input)
        {
            if(string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Replace multiple consecutive spaces with a single space, then trim
            return Regex.Replace(input.Trim(), @"\s+", " ");
        }
    }
}

