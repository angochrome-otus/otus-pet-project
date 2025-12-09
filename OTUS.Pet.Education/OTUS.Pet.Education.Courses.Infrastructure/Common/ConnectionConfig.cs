using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Infrastructure.Common
{
    public class ConnectionConfig
    {
        internal readonly string _connectionString;
        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        internal string ConnectionString => _connectionString;
        internal ConnectionConfig(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}