using DataComparer.Appinit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace DataComparer
{
    public class Program
    {
        static void Main(string[] args) => BootStrapper.BootApp(args);
    }
}
