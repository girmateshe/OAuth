using Ninject;
using Policy.Pets.Provider.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Policy.Pets.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public static IAuthorizationProvider AuthorizationProvider { get; set; }
        public static IKernel Kernel;

        public UserRole UserRole { get; set; }

        public ApiAuthorizeAttribute()
        {
            UserRole = UserRole.ReadOnly;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return AuthorizationProvider.IsAuthorized(
                new RequestContext(actionContext, Kernel.Get<IDebugContext>()),
                Kernel.Get<IUserProvider>(), UserRole);
        }
    }
}