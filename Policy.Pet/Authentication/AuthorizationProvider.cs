using Policy.Pets.Provider.Interfaces;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;

namespace Policy.Pets.Authentication
{
    public class AuthorizationProvider : IAuthorizationProvider
    {
        private ITokenProvider _tokenProvider;
        public AuthorizationProvider(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public async Task<bool> IsAuthorized()
        {
            string token = HttpContext.Current.Request.Headers["Authorization"];
            if(string.IsNullOrWhiteSpace(token))
            {
                var query = HttpContext.Current.Request.Url.Query;
                NameValueCollection qsColl = null;

                if (!string.IsNullOrWhiteSpace(query))
                {
                    qsColl = HttpUtility.ParseQueryString(query);
                }

                if (qsColl != null)
                {
                    token = qsColl["access_token"];
                }
            }
            else if (token.Length > 7)
            {
                //Ignore "Bearer " from the authorization header
                token = token.Substring(7);
            }

            if (string.IsNullOrWhiteSpace(token))
                return false;

            return await _tokenProvider.Validate(token);
        }
    }
}