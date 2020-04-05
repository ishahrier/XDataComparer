using DataComparer.DatabaseConnectors;
using DataComparer.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataComparer
{
    internal class BootStrapper
    {

        private IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            var config = LoadConfiguration();
            services.AddSingleton(config);

            services.AddTransient<IReadSettings, AppSettings>();
            services.AddSingleton<IOracleConnector, OracleConnector>();
            services.AddSingleton<ISqlServerConnector, SqlServerConnector>();

            services.AddTransient<App>();
            
            return services;
        }

        private IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        public static void BootApp(string[] args) => new BootStrapper().ConfigureServices().BuildServiceProvider().GetService<App>().Run(args);
    }
}
