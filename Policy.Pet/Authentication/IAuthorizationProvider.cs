using Policy.Pets.Provider.Interfaces;

namespace Policy.Pets.Authentication
{
    public interface IAuthorizationProvider
    {
        bool IsAuthorized(IRequestContext requestContext, IUserProvider userProvider, UserRole userRole);
    }
}
