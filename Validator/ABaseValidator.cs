using DataComparer.Extensions;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DataComparer.Validator
{
    public class ValueMatchingCandidate<T>
    {
        public KeyValuePair<string, T> RedsPair { get; set; } = new KeyValuePair<string, T>();
        public KeyValuePair<string, T> CobaltPair { get; set; } = new KeyValuePair<string, T>();

        public string Key => RedsPair.Key;
        public T RedsValue => RedsPair.Value;
    }
    public class DictionaryCompareResult<T>
    {
        public Dictionary<string, T> MismatchedKeys { get; set; }
        public List<ValueMatchingCandidate<T>> MismatchedValues { get; set; }

        public DictionaryCompareResult()
        {
            this.MismatchedKeys = new Dictionary<string, T>();
            this.MismatchedValues = new List<ValueMatchingCandidate<T>>();
        }

        public bool IsValid => MismatchedKeys.Any() == false && MismatchedValues.Any() == false;

    }

    public abstract class ABaseDataValidator : IValidateData, IDisposable
    {
        #region Protected members
        protected OracleConnection RedsCon;
        protected SqlConnection CobaltCon;
        protected Func<object, string> StringIndexConverter = (x => x == DBNull.Value ? "None" : x.ToString());
        protected Func<object, string> StringValueConverter = (x => x == DBNull.Value ? string.Empty : x.ToString());
        protected Func<object, decimal> DecimalValueConverter = (x => x == DBNull.Value ? 0 : Convert.ToDecimal(x));
        protected Func<ValueMatchingCandidate<string>, bool> StringValueComparer =
            ((candidate) => candidate.RedsPair.Value == candidate.CobaltPair.Value);
        protected Func<ValueMatchingCandidate<decimal>, bool> DecimalValueComparer =
            ((candidate) => candidate.RedsPair.Value == candidate.CobaltPair.Value);
        protected Func<ValueMatchingCandidate<DataRow>, DataColumnCollection, int> DefaultDataRowComparer =
            ((candidates, cols) =>
            {
                var rData = candidates.RedsPair;
                var cData = candidates.CobaltPair;
                var totalColError = 0;
                for (var cId = 0; cId < cols.Count; cId++)
                {
                    var col = cols[cId];
                    var rValue = rData.Value[col.ColumnName];
                    var cValue = cData.Value[col.ColumnName];
                    var noError = true;

                    if (rValue == DBNull.Value || cValue == DBNull.Value)
                    {
                        if (rValue != cValue) noError = false;
                    }
                    else if (col.DataType == typeof(string))
                    {
                        if (!rValue.ToString().Equals(cValue.ToString())) noError = false;
                    }
                    else if (col.DataType == typeof(Int32) || col.DataType == typeof(Int64) ||
                             col.DataType == typeof(Int64) || col.DataType == typeof(decimal) ||
                             col.DataType == typeof(int))
                    {
                        if (Convert.ToDecimal(rValue) != Convert.ToDecimal(cValue)) noError = false;
                    }
                    else
                        throw new Exception(
                            $"Invalid data type detected at key = '{candidates.Key}' col name = '{col.ColumnName}'");

                    if (!noError)
                    {
                        if (totalColError == 0) Console.WriteLine();
                        totalColError++;
                        Console.WriteLine(
                            $"Column value mismatch | Key: '{candidates.Key.Warning()}' | Col: '{cols[cId].ColumnName.Warning()}' | Source Val: '{rValue}' | Target Val: '{cValue}' ");
                    }
                }

                return totalColError;
            });
        #endregion

        #region Abstract methods to be implemented
        public abstract string GetRedsSql();
        public abstract string GetCobaltSql();
        public abstract ValidatorGroup GetGroup();
        public abstract string GetValidatorName();
        public abstract string GetValidatorDescription();
        #endregion

        #region Helpers

        #region Connection init helpers
        protected OracleConnection InitRedsOracleConnection()
        {
            const string tns = @"(DESCRIPTION =
	                        (ADDRESS = (PROTOCOL = TCP)(HOST = 10.188.2.3)(PORT = 1521))
	                        (CONNECT_DATA =
	                          (SERVER = DEDICATED)
	                          (SERVICE_NAME = pdb01.privsubnet2reg.vcnnewmarket01.oraclevcn.com)
	                        ))";

            if (OracleConfiguration.OracleDataSources["dev"] == "")
                OracleConfiguration.OracleDataSources.Add("dev", tns);

            var oc = new OracleConnection("user id=tdmuser; password=D8J8w43SQ_4h_JzJ1GbG; data source=dev");

            return oc;
        }
        protected SqlConnection InitCobaltSqlServerConnection()
        {
            const string conString = "Data Source=RICSQLSVR001TST;Initial Catalog=Cobalt_UAT;Integrated Security=True;MultipleActiveResultSets=True";
            var con = new SqlConnection(conString);

            return con;
        }
        #endregion

        #region Simple data comparison helpers

        protected Dictionary<string, T> LoadRedsDictionary<T>(string keyCol, string valCol, Func<object, T> valueConverter, Func<object, string> indexConverter = null) =>
            LoadRedsDictionary<T>(GetRedsSql(), keyCol, valCol, valueConverter, indexConverter ?? StringIndexConverter);

        protected Dictionary<string, T> LoadRedsDictionary<T>(string sql, string keyCol, string valCol, Func<object, T> valueConverter, Func<object, string> indexConverter)
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();
            using (var cmd = RedsCon.CreateCommand())
            {
                cmd.CommandText = sql;
                var ret = cmd.ExecuteReader();
                while (ret.Read())
                {
                    dict.Add(indexConverter(ret[keyCol]), valueConverter(ret[valCol]));
                }
            }

            return dict;
        }

        protected Dictionary<string, T> LoadCobaltDictionary<T>(string keyCol, string valCol, Func<object, T> valueConverter, Func<object, string> indexConverter = null) =>
            LoadCobaltDictionary<T>(GetCobaltSql(), keyCol, valCol, valueConverter, indexConverter ?? StringIndexConverter);

        protected Dictionary<string, T> LoadCobaltDictionary<T>(string sql, string keyCol, string valCol, Func<object, T> valueConverter, Func<object, string> indexConverter)
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();
            using (var cmd = CobaltCon.CreateCommand())
            {
                cmd.CommandText = sql;
                var ret = cmd.ExecuteReader();
                while (ret.Read())
                {
                    var record = (IDataRecord)ret;
                    dict.Add(indexConverter(record[keyCol]), valueConverter(ret[valCol]));
                }
            }
            return dict;
        }

        protected DictionaryCompareResult<T> CompareDictionaries<T>(Dictionary<string, T> reds, Dictionary<string, T> cobalt, Func<ValueMatchingCandidate<T>, bool> valueComparer = null)
        {
            var compareResult = new DictionaryCompareResult<T>();
            foreach (var d in reds.Keys)
            {
                if (!cobalt.ContainsKey(d))
                {
                    compareResult.MismatchedKeys.Add(d, reds[d]);
                }
                else
                {
                    if (valueComparer == null) continue;
                    var matchingPair = new ValueMatchingCandidate<T>()
                    {
                        CobaltPair = new KeyValuePair<string, T>(d, cobalt[d]),
                        RedsPair = new KeyValuePair<string, T>(d, reds[d])
                    };
                    if (!valueComparer(matchingPair)) compareResult.MismatchedValues.Add(matchingPair);
                }
            }
            return compareResult;
        }
        #endregion

        #region Simple record counts helpers
        protected bool DoCountValidation() => DoCountValidation(GetRedsSql(), GetCobaltSql());
        protected bool DoCountValidation(string redsCountSql, string cobaltCountSql)
        {
            var oCount = RedsDoCount(redsCountSql);
            var sCount = CobaltDoCount(cobaltCountSql);
            return ShowCountResult(oCount, sCount);
        }
        private int RedsDoCount(string sql)
        {
            var countSql = $"select count(*) from ({sql})";
            var count = 0;
            using (var cmd = RedsCon.CreateCommand())
            {
                cmd.CommandText = countSql;
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return count;
        }
        private int CobaltDoCount(string sql)
        {
            var countSql = $"select count(*) from ({sql}) as c";

            var count = 0;
            using (var cmd = CobaltCon.CreateCommand())
            {
                cmd.CommandText = countSql;
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return count;
        }
        private bool ShowCountResult(int redsCount, int cobaltCount)
        {
            bool noError = true;
            var result = redsCount == cobaltCount;
            if (result)
                Console.WriteLine($"{GetValidatorName()} COUNT RESULT: {"PASSED".SuccessWithBg()} | ReDS: {redsCount.ToString().Info()} | Cobalt: {cobaltCount.ToString().Info()}");
            else
            {
                Console.WriteLine($"{GetValidatorName()} COUNT RESULT: {"FAILED".ErrorWithBg()}  | ReDS: {redsCount.ToString().Info()} | Cobalt: {cobaltCount.ToString().Info()}");
                noError = false;
            }

            return noError;
        }
        #endregion

        #region Table records comparison helper

        protected DataSet GetRedSDataSet(string redsSql)
        {
            DataSet redS = new DataSet();

            using (var cmd = RedsCon.CreateCommand())
            {
                cmd.CommandText = redsSql;
                OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                adapter.Fill(redS);
            }

            return redS;
        }
        protected DataSet GetCobaltDataSet(string cobaltSql)
        {
            DataSet cobalt = new DataSet();

            using (var cmd = CobaltCon.CreateCommand())
            {
                cmd.CommandText = cobaltSql;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(cobalt);
            }

            return cobalt;
        }

        /// <summary>
        /// Compares two data sets. There must be a key present in both data sets and the columns must be same in both data sets.
        /// Columns don't have to be ordered but they should ne named same in both data sets.
        /// Data need not to be sorted either.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="cols"></param>
        /// <param name="dataRowComparer">Function to compare the columns for a data row in the given data sets</param>
        /// <returns></returns>
        protected DictionaryCompareResult<DataRow> CompareRecords(Dictionary<string, DataRow> source, Dictionary<string, DataRow> target, DataColumnCollection cols, Func<ValueMatchingCandidate<DataRow>, DataColumnCollection, int> dataRowComparer)
        {
            var result = new DictionaryCompareResult<DataRow>();
            foreach (var d in source.Keys)
            {
                if (target.ContainsKey(d))
                {
                    var candidates = new ValueMatchingCandidate<DataRow>()
                    {
                        CobaltPair = new KeyValuePair<string, DataRow>(d, target[d]),
                        RedsPair = new KeyValuePair<string, DataRow>(d, source[d])
                    };
                    var totalColValueMismatched = dataRowComparer(candidates, cols);

                    if (totalColValueMismatched > 0) result.MismatchedValues.Add(candidates);
                }
                else
                    result.MismatchedKeys.Add(d, source[d]);
            }
            Console.WriteLine();
            Console.WriteLine("Total unmatched KEYs: " + result.MismatchedKeys.Count.ToString().Info());
            Console.WriteLine("Total unmatched RECORDs: " + result.MismatchedValues.Count.ToString().Info());
            return result;
        }
        protected DictionaryCompareResult<DataRow> DoRecordValidation(Func<ValueMatchingCandidate<DataRow>, DataColumnCollection, int> dataRowComparer = null)
        {
            var rSet = GetRedSDataSet(GetRedsSql());
            var cSet = GetCobaltDataSet(GetCobaltSql());
            var rDict = rSet.Tables[0].AsEnumerable().ToDictionary<DataRow, string, DataRow>(row => row[0].ToString(), row => row);
            var cDict = cSet.Tables[0].AsEnumerable().ToDictionary<DataRow, string, DataRow>(row => row[0].ToString(), row => row);
            var result = CompareRecords(rDict, cDict, rSet.Tables[0].Columns, dataRowComparer ?? DefaultDataRowComparer);
            return result;
        }
        protected bool CompareSortedDataSetSnapShots(string redsSql, string cobaltSql)
        {
            DataSet redS = GetRedSDataSet(redsSql);
            DataSet cobalt = GetCobaltDataSet(cobaltSql);

            if (redS.Tables[0].Rows.Count == cobalt.Tables[0].Rows.Count)
            {
                var totalMismatch = 0;
                var cols = redS.Tables[0].Columns;

                for (var i = 0; i < redS.Tables[0].Rows.Count; i++)
                {
                    var rData = redS.Tables[0].Rows[i];
                    var cData = cobalt.Tables[0].Rows[i];
                    for (var cId = 0; cId < cols.Count; cId++)
                    {
                        if (!rData[cId].Equals(cData[cId]))
                        {
                            totalMismatch += 1;
                            Console.WriteLine("Data sets are not equal.".Error());
                            break;
                        }
                    }
                }

                return totalMismatch == 0;
            }
            else
            {
                Console.WriteLine("Snapshot could not be compared because row counts are different. Cannot continue...".ErrorWithBg());
                return false;
            }
        }
        #endregion
        #endregion

        #region constructor
        protected ABaseDataValidator()
        {
            RedsCon = InitRedsOracleConnection();
            CobaltCon = InitCobaltSqlServerConnection();
        }
        #endregion

        protected abstract bool StartValidation();
        public void Dispose()
        {
            if (RedsCon != null && RedsCon.State != ConnectionState.Closed) RedsCon.Close();
            if (CobaltCon != null && CobaltCon.State != ConnectionState.Closed) CobaltCon.Close();
        }
        public bool Run()
        {
            this.CobaltCon.Open();
            this.RedsCon.Open();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine($"***** {this.GetValidatorName()} started processing. *****".InfoWithBg());
            Console.WriteLine($"Description: {this.GetValidatorDescription()}".HotPink());
            var retRes = false;
            try
            {
                retRes = this.StartValidation();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred in {GetValidatorName().ErrorWithBg()}");
                Console.WriteLine(ex);
            }

            if (retRes) Console.WriteLine(GetValidatorName() + ":" + "PASSED".SuccessWithBg());
            else Console.WriteLine(GetValidatorName() + ": " + "FAILED".ErrorWithBg());
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var timeElapsed = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine($"***** { this.GetValidatorName()} finished processing. | Time elapsed : {timeElapsed}.  *****".InfoWithBg());
            this.Dispose();
            return retRes;
        }
    }
}

