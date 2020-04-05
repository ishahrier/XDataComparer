using DataComparer.Settings;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer.DatabaseConnectors
{
    public interface IOracleConnector
    {
        OracleConnection GetConnection(DataFlow setting);
        OracleConnection GetDefaultConnection();
         void TestConnection(OracleConnection con);
    }

 
}
