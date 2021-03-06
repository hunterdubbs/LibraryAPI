using LibraryAPI.Domain;
using LibraryAPI.Domain.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private SymmetricSecurityKey symmetricSecurityKey;
        private SigningCredentials signingCredentials;
        private JwtSecurityTokenHandler tokenHandler;

        public IdentityController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;                                                                                                                                                                                                                                                              
            this.configuration = configuration;
            symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT-SECRET-KEY"]));
            signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            tokenHandler = new JwtSecurityTokenHandler();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            string email = request.Email.Trim().ToLower();
            string username = request.Username.Trim().ToLower();

            ApplicationUser getByNameResult = await userManager.FindByNameAsync(username);
            if (getByNameResult != null) return BadRequest("an account with this username already exists");

            ApplicationUser getByEmailResult = await userManager.FindByEmailAsync(email);
            if (getByEmailResult != null) return BadRequest("an account with this email already exists");

            ApplicationUser user = new ApplicationUser()
            {
                UserName = username,
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            IdentityResult result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) return BadRequest("an account could not be made: " + string.Join(',', result.Errors.Select(e => e.Code + " - " + e.Description)));

            return Ok("User created");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            string username = request.Username.Trim().ToLower();

            ApplicationUser user = await userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("username or password incorrect");

            bool passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid) {
                await userManager.AccessFailedAsync(user);
                return BadRequest("username or password incorrect");
            }

            bool isLocked = await userManager.IsLockedOutAsync(user);
            if (isLocked) return BadRequest("account is locked");

            //login success
            List<Claim> claims = new List<Claim>(2);
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Id));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: configuration["BaseURL"],
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                audience: "all",
                signingCredentials: signingCredentials);

            return Ok(new { token = tokenHandler.WriteToken(token), expiration = token.ValidTo });
        }

        [HttpGet]
        [Authorize]
        [Route("test")]
        public IActionResult Test()
        {
            return Ok("Yee");
        }

    }
}
