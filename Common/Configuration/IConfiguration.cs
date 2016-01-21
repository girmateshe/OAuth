using System;
using System.Collections.Generic;

namespace Common.Configuration
{
    public enum DatabaseType
    {
        Pets,
        LocalDb
    }
    public interface IConfiguration
    {
        string RootRestApiUrl { get; set; }
        bool? IsAuthenticationEnabled { get; set; }
        string PrivateKey { get; set; }
        string PublicKey { get; set; }
        string Issuer { get; set; }
        IDictionary<DatabaseType, string> ConnectionStrings { get; }
    }
}
