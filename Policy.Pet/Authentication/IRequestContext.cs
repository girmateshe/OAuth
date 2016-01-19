using Policy.Pets.Provider.Interfaces;

namespace Policy.Pets.Authentication
{
    public interface IRequestContext
    {
        string UserName { get; set; }
        string Password { get; set; }
        string Token { get; set; }
        string Role { get; set; }
        string ClientId { get; set; }
        IDebugContext DebugContext { get; set; }
    }
}
