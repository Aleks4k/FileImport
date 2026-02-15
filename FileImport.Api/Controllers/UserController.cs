using FileImport.Application.Users.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileImport.Api.Controllers
{
    public class UserController : BaseController
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("signin-google")]
        public async Task<ActionResult> LogInWithGoogle(GoogleLoginRequestCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}
