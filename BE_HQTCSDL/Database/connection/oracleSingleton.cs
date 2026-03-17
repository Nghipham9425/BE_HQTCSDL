using Oracle.ManagedDataAccess.Client;

namespace BE_HQTCSDL.Database.connection
{
    public class OracleSingleton
    {
        private static OracleConnection _connection;
        private static readonly object _lock = new object();

        public static OracleConnection GetConnection()
        {
            if(_connection == null)
            {
                lock (_lock)
                {
                    if (_connection == null)
                    {
                        _connection = new OracleConnection(Config.Environment.OracleConnectionString);
                        _connection.Open();
                    }
                }
            }
            return _connection;
        }

    }
}