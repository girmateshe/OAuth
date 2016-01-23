using System;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Policy.Pets.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public static IAuthorizationProvider AuthorizationProvider { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            try {
                return AuthorizationProvider.IsAuthorized().Result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}