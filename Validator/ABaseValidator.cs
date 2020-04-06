using DataComparer.Extensions;
using DataComparer.Settings;
using System;
using System.Diagnostics;

namespace DataComparer.Validator
{
    public abstract class ABaseValidator : IDataValidator
    {
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
         
            return retRes;
        }

        #region Abstract methods
        public abstract string GetSourceSql();
        public abstract string GetTargetSql();
        public abstract string GetValidationGroup();
        public abstract DataBaseType SourceDbType();
        public abstract DataBaseType TargetDbType();
        protected abstract bool StartValidation();
        #endregion

        #region Overridable methods
        public virtual string GetCommandCode() => this.GetType().Name.GetCamelCaseLetters();
        public virtual string GetDescription() => this.GetType().Name;
        public virtual string GetName() => this.GetType().Name;
        #endregion

        #region Helpers
        #region Simple record counts helpers
        protected bool DoCountValidation() => DoCountValidation(GetSourceSql(), GetTargetSql());
        protected bool DoCountValidation(string redsCountSql, string cobaltCountSql)
        {
            var oCount = DoSourceCount(redsCountSql);
            var sCount = DoTargetCount(cobaltCountSql);
            return ShowCountResult(oCount, sCount);
        }
        private int DoSourceCount(string sql)
        {
            var countSql = $"select count(*) from ({sql})";
            var count = 0;
            //using (var cmd = RedsCon.CreateCommand())
            //{
            //    cmd.CommandText = countSql;
            //    count = Convert.ToInt32(cmd.ExecuteScalar());
            //}
            return count;
        }
        private int DoTargetCount(string sql)
        {
            var countSql = $"select count(*) from ({sql}) as c";

            var count = 0;
            //using (var cmd = CobaltCon.CreateCommand())
            //{
            //    cmd.CommandText = countSql;
            //    count = Convert.ToInt32(cmd.ExecuteScalar());
            //}

            return count;
        }
        private bool ShowCountResult(int sourceCount, int targetCount)
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
}
