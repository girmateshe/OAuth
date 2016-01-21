using System.Collections.Generic;
using System.Configuration;

namespace Common.Configuration
{
    public class Configuration : IConfiguration
    {
        public string RootRestApiUrl { get; set; }
        public bool? IsAuthenticationEnabled { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Issuer { get; set; }
        public IDictionary<DatabaseType, string> ConnectionStrings { get; private set; }

        public Configuration()
        {
            RootRestApiUrl = ConfigurationManager.AppSettings["RootRestApiUrl"];
            PrivateKey = ConfigurationManager.AppSettings["PrivateKey"];
            PublicKey = ConfigurationManager.AppSettings["PublicKey"];
            Issuer = ConfigurationManager.AppSettings["Issuer"];
            IsAuthenticationEnabled = ConfigurationManager.AppSettings["IsAuthenticationEnabled"].ConvertTo<bool>();

            ConnectionStrings = new Dictionary<DatabaseType, string>();

            string connectionString = ConfigurationManager.AppSettings["DB_PETS_CONNECT"],
                   pwd = ConfigurationManager.AppSettings["DB_PETS_CONNECT_PWD"];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                ConnectionStrings.Add(DatabaseType.Pets, string.Format(connectionString, pwd.ToPlainText()));
            }

            var connStrings = ConfigurationManager.ConnectionStrings["LocalPetsConnectionString"];

            if (connStrings != null)
            {
                connectionString = connStrings.ConnectionString;
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    ConnectionStrings.Add(DatabaseType.LocalDb, connectionString);
                }
            }
    
        }
    }
}
