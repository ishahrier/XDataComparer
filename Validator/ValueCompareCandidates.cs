using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer.Validator
{

    public class ValueCompareCandidates<T>
    {
        public KeyValuePair<string, T> RedsPair { get; set; } = new KeyValuePair<string, T>();
        public KeyValuePair<string, T> CobaltPair { get; set; } = new KeyValuePair<string, T>();

        public string Key => RedsPair.Key;
        public T RedsValue => RedsPair.Value;
    }
}
