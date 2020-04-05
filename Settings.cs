using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataComparer
{
    public class Settings : IReadSettings
    {
        private readonly IConfiguration config;

        public Settings(IConfiguration config) => this.config = config;
        public string GetConnectionString(DataFlow f) => config.GetConnectionString(f.ConnectionName);
        public DataFlowSettings DataFlow
        {
            get
            {
                var setting = new DataFlowSettings();
                config.GetSection("DataFlow").Bind(setting);
                return setting;
            }
        }

    }
}
