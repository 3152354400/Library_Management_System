using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace Backend.Data
{
    public interface IOracleConnectionFactory
    {
        Task<IDbConnection> CreateAsync();
    }

    public class OracleConnectionFactory : IOracleConnectionFactory
    {
        private readonly string _connStr;
        public OracleConnectionFactory(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("OracleDB")
                       ?? throw new InvalidOperationException("ConnectionStrings:Oracle 未配置");
        }
        
        public async Task<IDbConnection> CreateAsync()
        {
            var conn = new OracleConnection(_connStr);
            await conn.OpenAsync();
            return conn;
        }
    }
}