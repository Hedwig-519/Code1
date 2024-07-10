using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020362.BusinessLayers
{
    public static class Configuration
    {
        /// <summary>
        /// Chuỗi thông số kết nối đến CSDL
        /// </summary>
        public static string ConnectionString { get; set; } = "";
        public static void Initialize (string connectionString)
        {
            Configuration.ConnectionString = connectionString;
        }

        public static void Initialize(object connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
