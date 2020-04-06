using DataComparer.DatabaseConnectors;
using DataComparer.Settings;
using Pastel;
using System;
using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.SqlClient;

namespace DataComparer
{
    public class App
    {
        private readonly IReadSettings settings;

        public App(IReadSettings settings, IServiceProvider provider)
        {
            var con = provider.GetService<IDbConnector<SqlConnection>>();
            con.TestConnection(con.GetConnection());

            this.settings = settings;
        }

        public void Run(string[] args)
        {
            

            Console.WriteLine($"presee a {"key".Pastel(Color.Red)}");
        }
    }
}
