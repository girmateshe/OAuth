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
        IDictionary<DatabaseType, string> ConnectionStrings { get; }
    }
}
