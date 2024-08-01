using Agenda.Auth;
using Agenda.Data;
using Agenda.Models;
using Agenda.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Agenda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthenticateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AgendaDbContext _db;

        public AuthenticateController(IConfiguration configuration, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, AgendaDbContext db)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserViewModel model)
        {
            var userOnDb = await _userManager.FindByEmailAsync(model.Email);

            //se o usuário já existe retorna erro
            if (userOnDb is not null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseViewModel
                    {
                        Success = false,
                        Message = "Usuário já existe"
                    }
                );
            }

            //usuario nao existe
            IdentityUser newIdentityUser = new IdentityUser
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = model.Email,
                UserName = model.Name,
            };

            var result = await _userManager.CreateAsync(newIdentityUser, model.Password);

            //erro ao criar usuaio no banco
            if (!result.Succeeded) 
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseViewModel
                    {
                        Success = false,
                        Message = "Erro ao criar usuário"
                    }
                );
            }

            var user = new User
            {
                Email = model.Email,
                Password = model.Password,
                IdentityUserId = newIdentityUser.Id,
                Name = model.Name,
            };

            var role = UserRoles.User;
            if (model.IsAdm != null)
            {
                role = (bool) model.IsAdm ? UserRoles.Admin : UserRoles.User;
            }
                

            await AddRoleAsync(newIdentityUser, role);

            user.IdentityUserId = newIdentityUser.Id;

            try
            {
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                var par = "sd";
            }
            var userAuthClaims = await GetUserAuthClaims(newIdentityUser);
            var userToken = GetToken(userAuthClaims);

            return Ok(new ResponseViewModel { 
                Success = true, 
                Message = "Usuário criado com sucesso!", 
                TokenInfo =  userToken
            });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            bool isPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (user is not null && isPassword) 
            {
                var userEvents = await _db.Events.Where(e => e.User.IdentityUserId == user.Id && e.IsDeleted != true).ToListAsync();
                var authClaims = await GetUserAuthClaims(user);

                return Ok(new ResponseViewModel { Success = true, TokenInfo = GetToken(authClaims), Events = userEvents });
            }

            if(user is null || !isPassword)
            {
                return Ok(new ResponseViewModel
                {
                    Success = false,
                    Message = "Email e/ou senha inválidos!"
                });
            }

            return Unauthorized();
        }

        private TokenViewModel GetToken(List<Claim> authClaims) 
        {
            var authSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddDays(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new TokenViewModel 
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ValidTo = token.ValidTo
            };
        }

        private async Task<List<Claim>> GetUserAuthClaims(IdentityUser user)
        {
            var authClaims = new List<Claim>
            {
                new Claim (ClaimTypes.Name, user.UserName),
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim (ClaimTypes.Email, user.Email)
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

            return authClaims;
        }


        private async Task AddRoleAsync(IdentityUser user, string role)
        {
            //caso nao tenha a função a cria
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);
        }
    }
}
