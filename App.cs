using DataComparer.DatabaseConnectors;
using DataComparer.Settings;
using Pastel;
using System;
using System.Drawing;

namespace DataComparer
{
    public class App
    {
        private readonly IReadSettings settings;

        public App(IReadSettings settings)
        {

            this.settings = settings;
        }

        public void Run(string[] args)
        {
            

            Console.WriteLine($"presee a {"key".Pastel(Color.Red)}");
        }
    }
}
