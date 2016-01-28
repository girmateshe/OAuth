using System.Collections.Generic;
using System.Threading.Tasks;
using Policy.Pets.Models;

namespace Policy.Pets.Provider.Interfaces
{
    public interface ITokenProvider : IProvider<Token>
    {
        Task<Token> Generate(string userName, string password, string grant_type);
        Task<bool>Validate(string access_token);
        Task<TokenContent> Decode(string access_token);
    }
}
