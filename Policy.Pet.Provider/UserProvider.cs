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
        public UserProvider(IConfiguration configuration) : 
            base(configuration.ConnectionStrings[DatabaseType.LocalDb])
        {
            
        }

        public async Task<bool> Validate(string userName, string password)
        {
            return true;
        }

        public async override Task<IEnumerable<User>> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
