using DataComparer.Settings;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace DataComparer.DatabaseConnectors
{
    public interface IDbConnector<T> where T : DbConnection
    {
        T GetConnection(DataFlow setting);
        T GetConnection();
        void TestConnection(T con);
        DataBaseType GetDataBaseType();
        DataSet Fill(string sql,T con);
    }


}
