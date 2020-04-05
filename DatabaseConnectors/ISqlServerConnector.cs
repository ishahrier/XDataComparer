using DataComparer.Settings;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer.DatabaseConnectors
{
    public interface ISqlServerConnector
    {
        SqlConnection GetConnection(DataFlow setting);
        SqlConnection GetDefaultConnection();
        void TestConnection(SqlConnection con);
    }

   
}
