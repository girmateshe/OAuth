using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Configuration;
using Policy.Pets.Models;
using Policy.Pets.Provider.Interfaces;
using System;

namespace Policy.Pets.Provider
{
    public class UserProvider : BaseProvider<User> , IUserProvider
    {
        public UserProvider(IConfiguration configuration, IDebugContext debugContext) : 
            base(configuration.ConnectionStrings[DatabaseType.LocalDb])
        {
            DebugContext = debugContext;
        }

        public async Task<bool> Validate(string userName, string password)
        {
            //Validate 
            return true;
        }

        public async override Task<IEnumerable<User>> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
