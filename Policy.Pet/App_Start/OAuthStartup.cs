using Common;
using Common.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Policy.Pets.Provider;
using Policy.Pets.Provider.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

[assembly: OwinStartup(typeof(Policy.Pets.App_Start.OAuthStartup))]
namespace Policy.Pets.App_Start
{
    public class OAuthStartup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            ConfigureOAuth(app, new UserProvider(new Configuration()));
        }

        public void ConfigureOAuth(IAppBuilder app, IUserProvider userProvider)
        {
            var oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/v1/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(7),
                Provider = new SimpleAuthorizationServerProvider(userProvider)
            };


            app.Use(async (context, next) =>
            {
                if (context.Request.QueryString.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(context.Request.Headers.Get("Authorization")))
                    {
                        var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.Value);
                        string token = queryString.Get("access_token");

                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            context.Request.Headers.Add("Authorization", new[] { string.Format("Bearer {0}", token) });
                        }
                    }
                }

                await next.Invoke();
            });

            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

    }

    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private IUserProvider _userProvider;
        public SimpleAuthorizationServerProvider(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            var result = await _userProvider.Validate(context.UserName, context.Password);

            if (!result)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("uid", context.UserName));
            identity.AddClaim(new Claim("pw", context.Password.ToEncryptedText()));
            identity.AddClaim(new Claim("clientId", GetIp(true)));
            identity.AddClaim(new Claim("role", "user"));

            context.Validated(identity);

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

            HttpContext.Current.Trace.Write("Token Generation Client IP = " + ip);
            HttpContext.Current.Trace.Write("Token Generation HTTP_X_FORWARDED_FOR = " + HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]);

            return ip;
        }
    }

}