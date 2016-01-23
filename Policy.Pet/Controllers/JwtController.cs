using System.Threading.Tasks;
using System.Web.Http;
using Policy.Pets.Provider.Interfaces;
using Policy.Pets.Models;
using System;

namespace Policy.Pets.Controllers
{
    [RoutePrefix("oauth/v1/token")]
    public class JwtController : ApiController
    {
        private readonly ITokenProvider _tokenProvider;

        public JwtController(ITokenProvider tokenProvider, IDebugContext debugContext)
        {
            _tokenProvider = tokenProvider;
            _tokenProvider.DebugContext = debugContext;
        }

        [HttpPost, Route(""), AllowAnonymous]
        public async Task<IHttpActionResult> Create(TokenRequest token)
        {
            var accessToken = await _tokenProvider.Generate(token.username, token.password, token.grant_type);
            return Ok(accessToken);
        }

        [HttpGet, Route("validate")]
        public async Task<IHttpActionResult> Validate(string access_token)
        {
            bool valid = false;
            try
            {
                valid = await _tokenProvider.Validate(access_token);
            }
            catch(Exception ex)
            {
                valid = false;
            }

            return Ok(new { valid = valid });
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> DecodeToken(string access_token)
        {
            if(await _tokenProvider.Validate(access_token))
            {
                return Ok(await _tokenProvider.Decode(access_token));
            }

            return BadRequest();
        }

    }
}
