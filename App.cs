using DataComparer.DatabaseConnectors;
using DataComparer.Settings;
using DataComparer.Validator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Pastel;
using System;
using System.Drawing;

namespace DataComparer
{
    public class App
    {
        private readonly IReadSettings settings;

        public App(IReadSettings settings, IServiceProvider provider)
        {
            CustomersRecord c = new CustomersRecord(provider);
            c.Run();

            this.settings = settings;
        }

        public void Run(string[] args)
        {
            

            Console.WriteLine($"presee a {"key".Pastel(Color.Red)}");
        }
    }
}
