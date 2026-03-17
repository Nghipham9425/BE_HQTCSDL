using dotenv.net;

namespace BE_HQTCSDL.Config
{
    public class Environment
    {
        public static string OracleConnectionString => 
            System.Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING") ?? "";
        
        public static string JwtSecret => 
            System.Environment.GetEnvironmentVariable("JWT_SECRET") ?? "";
        
        public static string JwtIssuer => 
            System.Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "";
        
        public static string JwtAudience => 
            System.Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "";
        
        public static int JwtExpireHours => 
            int.Parse(System.Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS") ?? "24");
    }
}