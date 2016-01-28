using Policy.Pets.Provider.Interfaces;
using System.Threading.Tasks;

namespace Policy.Pets.Authentication
{
    public interface IAuthorizationProvider
    {
        Task<bool> IsAuthorized();
    }
}
