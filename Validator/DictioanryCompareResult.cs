using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataComparer.Validator
{
    public class DictionaryCompareResult<T>
    {
        public Dictionary<string, T> MismatchedKeys { get; set; }
        public List<ValueCompareCandidates<T>> MismatchedValues { get; set; }

        public DictionaryCompareResult()
        {
            this.MismatchedKeys = new Dictionary<string, T>();
            this.MismatchedValues = new List<ValueCompareCandidates<T>>();
        }

        public bool IsValid => MismatchedKeys.Any() == false && MismatchedValues.Any() == false;

    }
}
