using Common;
using Policy.Pets.Provider;
using Policy.Pets.Provider.Interfaces;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http.Controllers;

namespace Policy.Pets.Authentication
{
    public class RequestContext : IRequestContext
    {
        private HttpActionContext _actionContext;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public string ClientId { get; set; }
        public IDebugContext DebugContext { get; set; }

        public RequestContext(HttpActionContext actionContext, IDebugContext debugContext)
        {
            UserName = null;
            Password = null;
            DebugContext = debugContext;
            TryGetUserFromCurrentContext();
        }

        private void TryGetUserFromCurrentContext()
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            string username = null, password = null;
            var query = HttpContext.Current.Request.Url.Query;
            NameValueCollection qsColl = null;

            if (!string.IsNullOrWhiteSpace(query))
            {
                qsColl = HttpUtility.ParseQueryString(query);
            }

            try
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;

                if (identity != null && identity.Claims != null)
                {
                    var claim = identity.Claims.SingleOrDefault(c => c.Type == "uid");
                    if (claim != null)
                    {
                        username = claim.Value;
                    }

                    claim = identity.Claims.SingleOrDefault(c => c.Type == "pw");
                    if (claim != null)
                    {
                        password = claim.Value;
                        if (password != null) password = password.ToPlainText();
                    }

                    claim = identity.Claims.SingleOrDefault(c => c.Type == "role");
                    if (claim != null)
                    {
                        Role = claim.Value;
                    }

                    claim = identity.Claims.SingleOrDefault(c => c.Type == "clientId");
                    if (claim != null)
                    {
                        ClientId = claim.Value;
                    }
                }

            }
            catch (Exception ex)
            {
                if (qsColl != null)
                {
                    username = qsColl["uid"];
                    password = qsColl["pw"];
                }
            }

            if (qsColl != null &&
               (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password)))
            {
                username = qsColl["uid"];
                password = qsColl["pw"];
            }

            UserName = username;
            Password = password;
            string token = HttpContext.Current.Request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(token) && token.Length > 7)
            {
                //Ignore "Bearer " from the authorization header
                Token = token.Substring(7);
            }

            DebugContext.DebugMode = LogLevel.Off;

            if (qsColl != null)
            {
                var debug = qsColl["debug"];

                if (debug != null)
                {
                    LogLevel result;
                    LogLevel.TryParse(debug, true, out result);
                    DebugContext.DebugMode = result;
                }
            }

        }

    }
}