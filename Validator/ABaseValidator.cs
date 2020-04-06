using DataComparer.DatabaseConnectors;
using DataComparer.Extensions;
using DataComparer.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.Common;
using System.Diagnostics;

namespace DataComparer.Validator
{
    public abstract class ABaseValidator<T,TK> : IDisposable, IDataValidator where T:DbConnection where TK:DbConnection
    {
        private T sourceDbConnection;
        private TK targetDbConnection;
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


        #region Abstract methods
        
        public abstract string GetSourceSql();
        public abstract string GetTargetSql();
        public abstract string GetValidationGroup();

        protected abstract bool StartValidation();

        protected virtual DataBaseType GetSourceDataBaseType() => services.GetService<IDbConnector<T>>().GetDataBaseType();
        protected virtual DataBaseType GetTargetDataBaseType() => services.GetService<IDbConnector<TK>>().GetDataBaseType();
        protected virtual T GetSourceDBConnection()
        {
            if (sourceDbConnection == null)
                sourceDbConnection = services.GetService<IDbConnector<T>>().GetConnection();
            return sourceDbConnection;
        }
        protected virtual TK GetTargetDBConnection()
        {
            if (targetDbConnection == null)
                targetDbConnection = services.GetService<IDbConnector<TK>>().GetConnection();
            return targetDbConnection;
        }

        #endregion

        #region Overridable methods
        public virtual string GetCommandCode() => GetType().Name.GetCamelCaseLetters();
        public virtual string GetDescription() => GetType().Name;
        public virtual string GetName() => GetType().Name;
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
        protected int DoSourceCount(string sql)
        {
            var countSql = $"select count(*) from ({sql})";
            var count = 0;
            using (var cmd = GetSourceDBConnection().CreateCommand())
            {
                cmd.CommandText = countSql;
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return count;
        }
        protected int DoTargetCount(string sql)
        {
            var countSql = $"select count(*) from ({sql}) as c";

            var count = 0;
            using (var cmd = GetSourceDBConnection().CreateCommand())
            {
                cmd.CommandText = countSql;
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return count;
        }
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

        #endregion
    }


    public class CustomersCount : ABaseValidator<OracleConnection,SqlConnection>
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
