using DataComparer.DatabaseConnectors;
using DataComparer.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace DataComparer
{
    public class BootStrapper
    {

        private IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            var config = LoadConfiguration();
            services.AddSingleton(config);

            services.AddTransient<IReadSettings, AppSettings>();
            services.AddSingleton<IDbConnector<OracleConnection>, OracleConnector>();
            services.AddSingleton<IDbConnector<SqlConnection>, SqlServerConnector>();

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
