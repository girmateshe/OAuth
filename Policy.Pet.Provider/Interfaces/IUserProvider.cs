using System.Collections.Generic;
using System.Threading.Tasks;
using Policy.Pets.Models;

namespace Policy.Pets.Provider.Interfaces
{
    public interface IUserProvider : IProvider<User>
    {
        Task<bool> Validate(string userName, string password);
    }
}
