using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer
{
    public class DataFlowSettings
    {
        public DataFlow Source { get; set; }
        public DataFlow Target { get; set; }
    }

    public class DataFlow
    {
        public DataFlow()
        {
        }
        public string DbType { get; set; }
        public string ConnectionName { get; set; }

 
    }
}
