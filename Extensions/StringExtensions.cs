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
            int count = 1;
            for (int i = 1; i < input.Length - 1; i++)
            {
                if (input[i] >= 65 && input[i] <= 90)
                    chars.Add(input[i]);
            }

            return new string(chars.ToArray());

        }
    }
}
