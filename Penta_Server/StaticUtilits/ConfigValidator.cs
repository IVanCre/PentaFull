namespace Penta_Server.Utilits
{
    public static class ConfigValidator//чтобы не делать 100500 однотипных проверок повсюду
    {
        public static void Check(IConfiguration config)
        {
            CheckFirebaseFile();
            CheckCertificateFile();
            CheckUrls(config);
            CheckLogs(config);
            CheckJwt(config);
            CheckWorkDB(config);
            CheckDBCleaner(config);
        }
        private static void CheckFirebaseFile()
        {
            if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "penta-client-1928-firebase-adminsdk-fbsvc-6fb6a671e1.json")))
                throw new FileLoadException("Firebase SDK file not found");
        }
        private static void CheckCertificateFile()
        {
            if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "pentaPassCert.pfx")))
                throw new FileLoadException("Certificate file not found");
        }

        private static void CheckUrls(IConfiguration config)
        {
            //string http_url = config["Kestrel:Endpoints:Http:Url"];
            //if(Uri.TryCreate(http_url, UriKind.RelativeOrAbsolute, out Uri? _)==false)
            //    throw new ArgumentException("Http url invalid");

            string https_url = config["Kestrel:Endpoints:Https:Url"];
            if (Uri.TryCreate(https_url, UriKind.RelativeOrAbsolute, out Uri? _) == false)
                throw new ArgumentException("Https url invalid");
        }

        private static void CheckLogs(IConfiguration config)
        {
            var autodelete = config["Logging:AutoDeleteIntervalMinutes"];
            if (string.IsNullOrEmpty(autodelete))
                throw new ArgumentException("Invalid Logging AutoDeleteIntervalMinutes");
            int val=int.Parse(autodelete);
            if(val<1)
                throw new ArgumentException("Logging AutoDeleteIntervalMinutes should be >0");

            var days =config["Logging:MaxDayToHold"];
            if (string.IsNullOrEmpty(days))
                throw new ArgumentException("Invalid Logging MaxDayToHold");
            int val2 = int.Parse(days);
            if (val2 < 1)
                throw new ArgumentException("Logging MaxDayToHold should be >0");

            var folder = config["Logging:FolderName"];
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentException("Invalid Logging FolderName");
        }

        private static void CheckJwt(IConfiguration config)
        {
            string key = config["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid jwt key");

            string issuer= config["Jwt:Issuer"];
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentException("Invalid jwt issuer");

            string audience = config["Jwt:Audience"];
            if (string.IsNullOrEmpty(audience))
                throw new ArgumentException("Invalid jwt audience");

            string tokenLifetime = config["Jwt:LifeTimeMinutes"];
            if (string.IsNullOrEmpty(tokenLifetime))
                throw new ArgumentException("Invalid jwt LifeTimeMinutes");
            int period =int.Parse(tokenLifetime);
            if (period < 1)
                throw new ArgumentException("jwt LifeTimeMinutes should be >1");
        }

        private static void CheckWorkDB(IConfiguration config)
        {
            string connStr = config["WorkDB:ConnString"];
            if (string.IsNullOrEmpty(connStr))
                throw new ArgumentException("Invalid WorkDB ConnString");
        }

        private static void CheckDBCleaner(IConfiguration config)
        {
            string autoCleanDB = config["DBCleaner:AutoCleanPeriodMinutes"];
            if (string.IsNullOrEmpty(autoCleanDB))
                throw new ArgumentException("Invalid DBCleaner AutoCleanPeriodMinutes");
            int period =int.Parse(autoCleanDB);
        }
    }
}
