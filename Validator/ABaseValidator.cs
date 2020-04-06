using DataComparer.DatabaseConnectors;
using DataComparer.Extensions;
using DataComparer.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace DataComparer.Validator
{
    /// <summary>
    /// A Base database data validator
    /// </summary>
    /// <typeparam name="S">Source database connection i.e OracleConnection</typeparam>
    /// <typeparam name="T">Target adtabase connection i.e. SqlConnection</typeparam>
    public abstract class ABaseValidator<S, T> : IDisposable, IDataValidator where S : DbConnection where T : DbConnection
    {
        /// <summary>
        /// Source database connection
        /// </summary>
        private S sourceDbConnection;
        /// <summary>
        /// Target database connection
        /// </summary>
        private T targetDbConnection;
        private readonly IServiceProvider services;

        public ABaseValidator(IServiceProvider services) => this.services = services;

        public bool Run()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine($"***** {this.GetName()} started processing. *****".InfoWithBg());
            Console.WriteLine($"Description: {this.GetDescription()}".HotPink());
            var retRes = false;
            try
            {
                retRes = this.StartValidation();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred in {GetName().ErrorWithBg()}");
                Console.WriteLine(ex);
            }

            if (retRes) Console.WriteLine(GetName() + ":" + "PASSED".SuccessWithBg());
            else Console.WriteLine(GetName() + ": " + "FAILED".ErrorWithBg());
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var timeElapsed = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine($"***** { this.GetName()} finished processing. | Time elapsed : {timeElapsed}.  *****".InfoWithBg());
            Dispose();
            return retRes;
        }


        #region Private methods
        private IDbConnector<S> GetSourceConnector() => services.GetService<IDbConnector<S>>();
        private IDbConnector<T> GetTargetConnector() => services.GetService<IDbConnector<T>>();

        #endregion

        #region Abstract methods

        public abstract string GetSourceSql();
        public abstract string GetTargetSql();
        public abstract string GetValidationGroup();
        protected abstract bool StartValidation();

        #endregion

        #region Overridable methods
        public virtual string GetCommandCode() => GetType().Name.GetCamelCaseLetters();
        public virtual string GetDescription() => GetType().Name;
        public virtual string GetName() => GetType().Name;

        protected virtual DataBaseType GetSourceDataBaseType() => GetSourceConnector().GetDataBaseType();
        protected virtual DataBaseType GetTargetDataBaseType() => GetTargetConnector().GetDataBaseType();
        protected virtual S GetSourceDBConnection()
        {
            if (sourceDbConnection == null)
            {
                sourceDbConnection = GetSourceConnector().GetConnection();
                sourceDbConnection.Open();
            }
            return sourceDbConnection;
        }
        protected virtual T GetTargetDBConnection()
        {
            if (targetDbConnection == null)
            {
                targetDbConnection = GetTargetConnector().GetConnection();
                targetDbConnection.Open();
            }
            return targetDbConnection;
        }
        public virtual void Dispose()
        {
            if (sourceDbConnection != null && sourceDbConnection.State != System.Data.ConnectionState.Closed) sourceDbConnection.Close();
            if (targetDbConnection != null && targetDbConnection.State != System.Data.ConnectionState.Closed) targetDbConnection.Close();
        }
        #endregion

        #region Validation Helpers

        #region Simple record counts helpers
        protected bool DoCountValidation() => DoCountValidation(GetSourceSql(), GetTargetSql());
        protected bool DoCountValidation(string sourceCountSql, string targetCountSql)
        {
            var oCount = DoSourceCount(sourceCountSql);
            var sCount = DoTargetCount(targetCountSql);
            return DisplayCountResult(oCount, sCount);
        }
        /// <summary>
        /// Wraps up any sql with count sql keyword for counting the total records returned by the original select sql
        /// </summary>
        /// <param name="sql">Original select SQL</param>
        /// <returns>total number of records</returns>
        protected int DoSourceCount(string sql)
        {
            var countSql = $"select count(*) from ({sql})";
            if (this.GetSourceDataBaseType() == DataBaseType.SqlServer) countSql += " as c";
            var count = 0;
            using (var cmd = GetSourceDBConnection().CreateCommand())
            {
                cmd.CommandText = countSql;
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return count;
        }
        /// <summary>
        /// Wraps up any sql with count sql keyword for counting the total records returned by the original select sql
        /// </summary>
        /// <param name="sql">Original select SQL</param>
        /// <returns>total number of records</returns>
        protected int DoTargetCount(string sql)
        {
            var countSql = $"select count(*) from ({sql})";
            if (this.GetSourceDataBaseType() == DataBaseType.SqlServer) countSql += " as c";
            var count = 0;
            using (var cmd = GetSourceDBConnection().CreateCommand())
            {
                cmd.CommandText = countSql;
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return count;
        }
        /// <summary>
        /// Displays the formatted count record result.
        /// </summary>
        /// <param name="sourceCount">Total records found in source DB</param>
        /// <param name="targetCount">Total records found in source DB</param>
        /// <returns>True if total numbers are equal in both DB, False otherwise.</returns>
        protected bool DisplayCountResult(int sourceCount, int targetCount)
        {
            bool noError = true;
            var result = sourceCount == targetCount;
            if (result)
                Console.WriteLine($"{GetName()} COUNT RESULT: {"PASSED".SuccessWithBg()} | Source: {sourceCount.ToString().Info()} | Target: {targetCount.ToString().Info()}");
            else
            {
                Console.WriteLine($"{GetName()} COUNT RESULT: {"FAILED".ErrorWithBg()}  | Source: {sourceCount.ToString().Info()} | Target: {targetCount.ToString().Info()}");
                noError = false;
            }

            return noError;
        }
        #endregion

        #region Table records comparison helper

        protected DataSet GetSourceDataSet(string sourceSql)
        {
            DataSet sourceDataSet =  GetSourceConnector().Fill(sourceSql, sourceDbConnection);
            return sourceDataSet;
        }
        protected DataSet GetTargetDataSet(string targetSql)
        {
            DataSet targetDs =  GetTargetConnector().Fill(targetSql,targetDbConnection);
            return targetDs;
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
        protected DictionaryCompareResult<DataRow> CompareRecords(Dictionary<string, DataRow> source, Dictionary<string, DataRow> target, DataColumnCollection cols, Func<ValueCompareCandidates<DataRow>, DataColumnCollection, int> dataRowComparer)
        {
            var result = new DictionaryCompareResult<DataRow>();
            foreach (var d in source.Keys)
            {
                if (target.ContainsKey(d))
                {
                    var candidates = new ValueCompareCandidates<DataRow>()
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
        protected DictionaryCompareResult<DataRow> DoRecordValidation(Func<ValueCompareCandidates<DataRow>, DataColumnCollection, int> dataRowComparer = null)
        {
            var rSet = GetSourceDataSet(GetSourceSql());
            var cSet = GetTargetDataSet(GetTargetSql());
            var rDict = rSet.Tables[0].AsEnumerable().ToDictionary<DataRow, string, DataRow>(row => row[0].ToString(), row => row);
            var cDict = cSet.Tables[0].AsEnumerable().ToDictionary<DataRow, string, DataRow>(row => row[0].ToString(), row => row);
            var result = CompareRecords(rDict, cDict, rSet.Tables[0].Columns, dataRowComparer ?? DefaultDataRowComparer);
            return result;
        }
        
        #endregion

        #endregion

        #region Funcs<>s
        protected   Func<ValueCompareCandidates<DataRow>, DataColumnCollection, int> DefaultDataRowComparer =
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

    }


    public class CustomersCount : ABaseValidator<OracleConnection, SqlConnection>
    {
        public CustomersCount(IServiceProvider service) : base(service)
        {

        }
        public override string GetSourceSql() => @"select * from customers";

        public override string GetTargetSql() => "Select * from customers";

        public override string GetValidationGroup() => "RedsToTdm";


        protected override bool StartValidation()
        {
            return DoCountValidation();
        }
    }
}
