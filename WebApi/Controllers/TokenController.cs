using Application.Features.identity.Tokens;
using Application.Features.identity.Tokens.Queries;
using Infrastructure.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : BaseApiController
    {
        [HttpPost("login")]
        [AllowAnonymous]
        [TenantHeader]
        [OpenApiOperation(("Used to obtain jwt for login."))]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequest tokenRequest)

        {
            var response = await Sender.Send(new GetTokenQuery { TokenRequest = tokenRequest });
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [TenantHeader]
        [OpenApiOperation(("Used to obtain jwt for refresh."))]
        public async Task<IActionResult> GetRefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var response = await Sender.Send(new GetRefreshTokenQuery{RefreshToken = refreshTokenRequest});
            if (!response.IsSuccessful)
            { 
                return BadRequest(response);
            }
            return Ok(response);
        }
        
    }
}
