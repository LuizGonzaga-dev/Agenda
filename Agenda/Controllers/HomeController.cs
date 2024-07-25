using Agenda.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Agenda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Route("anonymous")]
        [AllowAnonymous]
        public string GetAnonymous() => "Anônimo";

        [HttpGet]
        [Route("user")]
        [Authorize(Roles = UserRoles.User)]
        public string GetUser() => "User";
    }
}
