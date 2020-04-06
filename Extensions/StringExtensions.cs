using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer.Extensions
{
    public static class StringExtensions
    {
        public static string GetCamelCaseLetters(this string input)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input)) return string.Empty;

            List<char> chars = new List<char>();            
            for (int i = 0; i < input.Length ; i++)
            {
                if (input[i] >= 65 && input[i] <= 90)
                    chars.Add(input[i]);
            }

            return new string(chars.ToArray());

        }
    }
}
