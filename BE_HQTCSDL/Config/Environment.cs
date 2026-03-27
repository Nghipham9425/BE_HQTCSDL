using dotenv.net;
using System;
using System.Linq;

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

        public static int RefreshTokenExpireDays =>
            int.Parse(System.Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRE_DAYS") ?? "7");
        
        public static int JwtExpireHours => 
            int.Parse(System.Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS") ?? "24");

        public static string[] FrontendOrigins
        {
            get
            {
                var raw = global::System.Environment.GetEnvironmentVariable("FRONTEND_ORIGINS");
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return ["http://localhost:3000"];
                }

                return raw
                    .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
                    .Select(origin => origin.Trim())
                    .Where(origin => !string.IsNullOrWhiteSpace(origin))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }
    }
}