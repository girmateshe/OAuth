using Policy.Pets.Provider.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Policy.Pets.Authentication
{
    public class AuthorizationProvider : IAuthorizationProvider
    {
        public bool IsAuthorized(IRequestContext requestContext, IUserProvider userProvider, UserRole userRole)
        {
            if (userRole == UserRole.None)
            {
                userRole = UserRole.ReadOnly;
            }

            string userName = requestContext.UserName,
                   password = requestContext.Password;

            if (userName == null || password == null)
            {
                return false;
            }

            var ip = GetIp(true);
            /*
            if (ip != requestContext.ClientId)
            {
                return false;
            }
             * */

            HttpContext.Current.Trace.Write("requestContext.ClientId = " + requestContext.ClientId);
            HttpContext.Current.Trace.Write("ip = " + ip);

            var result = userProvider.Validate(userName, password);

            return result != null && result.Result;
        }

        private bool IsUserInRole(IRequestContext requestContext, UserRole role)
        {
            return true;
        }

        private bool IsAuthenticatedContext(IRequestContext requestContext)
        {
            return true;
        }

        public string GetIp(bool checkForward = false)
        {
            string ip = null;
            if (checkForward)
            {
                ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            { // Using X-Forwarded-For last address
                ip = ip.Split(',')
                       .Last()
                       .Trim();
            }

            HttpContext.Current.Trace.Write("Client IP = " + ip);
            HttpContext.Current.Trace.Write("HTTP_X_FORWARDED_FOR = " + HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]);
            HttpContext.Current.Trace.Write("REMOTE_ADDR = " + HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
            HttpContext.Current.Trace.Write("UserHostAddress = " + HttpContext.Current.Request.UserHostAddress);

            return ip;
        }

    }
}