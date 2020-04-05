using DataComparer.Settings;
using Microsoft.Extensions.Configuration;
using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
            Console.WriteLine(this.settings.GetConnectionString(this.settings.DataFlow.Source));
            Console.WriteLine($"presee a {"key".Pastel(Color.Red)}");
        }
    }
}
