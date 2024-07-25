﻿using Agenda.Auth;
using Agenda.Data;
using Agenda.Models;
using Agenda.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Agenda.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<IActionResult> CreateUserAsync([FromBody] User model)
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
            IdentityUser newUser = new IdentityUser
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = model.Email,
                UserName = model.Name,
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            //erro ao criae usuanio no banco
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

            var role = model.IsAdm ? UserRoles.Admin : UserRoles.User;

            await AddRoleAsync(newUser, role);

            model.IdentityUserId = newUser.Id;

            await _db.Users.AddAsync(model);
            await _db.SaveChangesAsync();

            return Ok(new ResponseViewModel { Success = true, Message = "Usuário criado com sucesso!" });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            bool isPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (user is not null && isPassword) 
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

                return Ok(new ResponseViewModel { Success = true, Data = GetToken(authClaims) });
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


        private async Task AddRoleAsync(IdentityUser user, string role)
        {
            //caso nao tenha a função a cria
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);
        }
    }
}
