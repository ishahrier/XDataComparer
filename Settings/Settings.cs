using Microsoft.Extensions.Configuration;

namespace DataComparer.Settings
{
    public class AppSettings : IReadSettings
    {
        private readonly IConfiguration config;

        public AppSettings(IConfiguration config) => this.config = config;
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
