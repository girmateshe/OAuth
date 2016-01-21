using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Script.Serialization;
using Common;
using Common.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Policy.Pets.Provider.Interfaces;

namespace Policy.Pets.Controllers
{
    [Route("oauth/v1/token")]
    public class JwtController : ApiController
    {
        private readonly IUserProvider _userProvider;
        private readonly IConfiguration _configuration;

        public JwtController(IUserProvider userProvider, IDebugContext debugContext, IConfiguration configuration)
        {
            _userProvider = userProvider;
            _userProvider.DebugContext = debugContext;
            this._configuration = configuration;
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateToken(Token token)
        {
            var publicAndPrivate = new RSACryptoServiceProvider();
            
            publicAndPrivate.FromXmlString(_configuration.PrivateKey.FromBase64String());
            var jwtToken = new JwtSecurityToken(
                                issuer: _configuration.Issuer, 
                                audience: "http://mysite.com"
                                , claims: new List<Claim>() { new Claim(ClaimTypes.Name, token.username) }
                                , notBefore: DateTime.UtcNow
                                , expires: DateTime.UtcNow.AddMinutes(1)
                                , signingCredentials: new SigningCredentials(
                                    new RsaSecurityKey(publicAndPrivate)
                                       ,SecurityAlgorithms.RsaSha256Signature
                                       ,SecurityAlgorithms.Sha256Digest)
                           );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(jwtToken);

            return Ok(new
            {
                access_token = tokenString,
                expires_in = new TimeSpan(0,0, 1,0).TotalSeconds,
                expires_on = (long)(DateTime.UtcNow.AddMinutes(1) - new DateTime(1970, 1, 1)).TotalSeconds
            });
        }

        [HttpGet]
        public IHttpActionResult DecodeToken(string access_token)
        {
            var tokenReceived = new JwtSecurityToken(access_token);

            var publicOnly = new RSACryptoServiceProvider();
            publicOnly.FromXmlString(_configuration.PublicKey.FromBase64String());
            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _configuration.Issuer
               ,ValidAudience = "http://mysite.com"
               ,IssuerSigningToken = new RsaSecurityToken(publicOnly)
               ,ValidateLifetime = true
            };

            var recipientTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var claimsPrincipal = recipientTokenHandler.ValidateToken(access_token, validationParameters, out securityToken);

            var currentTime = (long) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            if (tokenReceived.Payload.Exp < currentTime)
            {
                throw new SecurityTokenValidationException(string.Format("Lifetime validation failed. The token is expired. ValidTo: '{0}' Current time: '{1}'.", tokenReceived.ValidTo, DateTime.UtcNow));
            }
          
            return Ok(new
            {
                header = tokenReceived.Header,
                payload = tokenReceived.Payload,
                current = currentTime
            });
        }

        private static RsaKeyGenerationResult GenerateRsaKeys()
        {
            var myRsa = new RSACryptoServiceProvider(2048);
            var publicKey = myRsa.ExportParameters(true);
            var result = new RsaKeyGenerationResult
            {
                PublicAndPrivateKey = myRsa.ToXmlString(true),
                PublicKeyOnly = myRsa.ToXmlString(false)
            };


            return result;
        }

    }

    public class RsaKeyGenerationResult
    {
        public string PublicKeyOnly { get; set; }
        public string PublicAndPrivateKey { get; set; }
    }

    public class Token
    {
        public string username { get; set; }
        public string password { get; set; }
        public string grant_type { get; set; }
    }

}
