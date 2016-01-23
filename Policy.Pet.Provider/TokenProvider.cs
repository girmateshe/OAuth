using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Configuration;
using Policy.Pets.Models;
using Policy.Pets.Provider.Interfaces;
using System;
using System.Security.Cryptography;
using System.IdentityModel.Tokens;
using Common;
using System.Security.Claims;

namespace Policy.Pets.Provider
{
    public class TokenProvider : BaseProvider<Token> , ITokenProvider
    {
        private IConfiguration _configuration;
        private IUserProvider _userProvider;

        public TokenProvider(IConfiguration configuration, IUserProvider userProvider, IDebugContext debugContext) : 
            base(configuration.ConnectionStrings[DatabaseType.LocalDb])
        {
            _configuration = configuration;
            _userProvider = userProvider;
            DebugContext = debugContext;
        }
        public async Task<Token> Generate(string userName, string password, string grant_type)
        {
            if(! await _userProvider.Validate(userName, password))
            {
                throw new Exception("The user name or password is incorrect.");
            }

            var publicAndPrivate = new RSACryptoServiceProvider();

            publicAndPrivate.FromXmlString(_configuration.PrivateKey.FromBase64String());
            var jwtToken = new JwtSecurityToken(
                                issuer: _configuration.Issuer,
                                audience: "http://mysite.com"
                                , claims: new List<Claim>() { new Claim(ClaimTypes.Name, userName) }
                                , notBefore: DateTime.UtcNow
                                , expires: DateTime.UtcNow.AddHours(1)
                                , signingCredentials: new SigningCredentials(
                                    new RsaSecurityKey(publicAndPrivate)
                                       , SecurityAlgorithms.RsaSha256Signature
                                       , SecurityAlgorithms.Sha256Digest)
                           );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(jwtToken);

            var token = new Token
            {
                access_token = tokenString,
                expires_in = new TimeSpan(0, 0, 1, 0).TotalSeconds,
                expires_on = (long)(DateTime.UtcNow.AddHours(1) - new DateTime(1970, 1, 1)).TotalSeconds
            };
            return token;
        }

        public async Task<bool> Validate(string access_token)
        {
            var tokenReceived = new JwtSecurityToken(access_token);

            var publicOnly = new RSACryptoServiceProvider();
            publicOnly.FromXmlString(_configuration.PublicKey.FromBase64String());
            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _configuration.Issuer,
                ValidAudience = "http://mysite.com",
                IssuerSigningToken = new RsaSecurityToken(publicOnly),
                ValidateLifetime = true
            };

            var recipientTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var claimsPrincipal = recipientTokenHandler.ValidateToken(access_token, validationParameters, out securityToken);

            var currentTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            if (tokenReceived.Payload.Exp < currentTime)
            {
                throw new SecurityTokenValidationException(string.Format("Lifetime validation failed. The token is expired. ValidTo: '{0}' Current time: '{1}'.", tokenReceived.ValidTo, DateTime.UtcNow));
            }

            return true;
        }

        public async Task<TokenContent> Decode(string access_token)
        {
            var tokenReceived = new JwtSecurityToken(access_token);
            
            return new TokenContent
            {
                Header = tokenReceived.Header,
                Payload = tokenReceived.Payload,
                Current = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds
            };
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

        public async override Task<IEnumerable<Token>> GetAll()
        {
            throw new NotImplementedException();
        }
    }

    public class RsaKeyGenerationResult
    {
        public string PublicKeyOnly { get; set; }
        public string PublicAndPrivateKey { get; set; }
    }
}
