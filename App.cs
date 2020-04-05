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
        private readonly IConfiguration _config;
        public App(IConfiguration config)
        {
            _config = config;
        }

        public void Run(string[] args)
        {
            Console.WriteLine($"presee a {"key".Pastel(Color.Red)}");
        }
    }
}
