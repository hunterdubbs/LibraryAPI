using LibraryAPI.Domain;
using LibraryAPI.Domain.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
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
        private TokenValidationParameters validationParameters;
        private string tokenIssuer;
        private readonly IEmailService emailService;

        public IdentityController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IEmailService emailService)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;                                                                                                                                                                                                                                                              
            this.configuration = configuration;
            this.emailService = emailService;
            tokenIssuer = configuration["BaseURL"]; //TODO: enable in prod
            symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecretKey"]));
            signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            tokenHandler = new JwtSecurityTokenHandler();
            validationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = tokenIssuer, 
                IssuerSigningKey = symmetricSecurityKey,
                ValidateAudience = false
            };
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

            string emailVerificationCode = await userManager.GenerateEmailConfirmationTokenAsync(user);
            string verificationUrl = string.Format("{0}/api/identity/verifyemail?token={1}&id={2}", GlobalSettings.BASE_URL, Base64Encode(emailVerificationCode), user.Id);
            emailService.SendEmail(user.Email, "Email Confirmation", 
                @"Welcome to Yet Another Library App!

Use the lnik below to confirm your email and activate your account

" + verificationUrl);

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

            if (!user.EmailConfirmed) return BadRequest("Email verification not complete");

            //bool isLocked = await userManager.IsLockedOutAsync(user);
            //if (isLocked) return BadRequest("account is locked");

            //login success
            List<Claim> claims = new List<Claim>(3);
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Id));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                audience: "all",
                signingCredentials: signingCredentials);

            List<Claim> refreshClaims = new List<Claim>(2);
            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Id));
            claims.Add(new Claim("TokenType", "REFRESH"));

            JwtSecurityToken refreshToken = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: refreshClaims,
                expires: DateTime.Now.AddDays(30),
                audience: "all",
                signingCredentials: signingCredentials);

            return Ok(new { 
                id = user.Id, 
                token = tokenHandler.WriteToken(token), 
                refreshToken = tokenHandler.WriteToken(refreshToken),
                expiration = token.ValidTo, 
                username = user.UserName 
            });
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshAuth([FromBody] RefreshRequest request)
        {
            //validate refresh token
            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(request.RefreshToken, validationParameters, out SecurityToken validatedToken);
                string tokenType = claimsPrincipal.FindFirstValue("TokenType");
                if(tokenType == null || tokenType.ToUpper() != "REFRESH") return BadRequest();
            }catch(Exception)
            {
                return BadRequest();
            }

            //get user and check that they're valid
            ApplicationUser user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return BadRequest();
            bool isLocked = await userManager.IsLockedOutAsync(user);
            if (isLocked) return BadRequest();

            //generate new tokens
            List<Claim> claims = new List<Claim>(3);
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Id));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                audience: "all",
                signingCredentials: signingCredentials);

            List<Claim> refreshClaims = new List<Claim>(2);
            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Id));
            claims.Add(new Claim("TokenType", "REFRESH"));

            JwtSecurityToken refreshToken = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: refreshClaims,
                expires: DateTime.Now.AddDays(30),
                audience: "all",
                signingCredentials: signingCredentials);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                refreshToken = tokenHandler.WriteToken(refreshToken)
            });
        }

        [HttpGet]
        [Route("test")]
        public IActionResult Test()
        {
            return Ok("Yee");
        }

        [HttpPost]
        [Route("emailverification/resend")]
        public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationRequest request)
        {
            string email = request.Email.Trim().ToLower();

            ApplicationUser user = await userManager.FindByEmailAsync(email);
            if (user == null) return Ok();

            string emailVerificationCode = await userManager.GenerateEmailConfirmationTokenAsync(user);
            string verificationUrl = string.Format("{0}/api/identity/verifyemail?token={1}&id={2}", GlobalSettings.BASE_URL, Base64Encode(emailVerificationCode), user.Id);
            emailService.SendEmail(user.Email, "Email Confirmation",
                @"Welcome to Yet Another Library App!

Use the code below in the app to confirm your email and activate your account

" + verificationUrl);

            return Ok();
        }

        [HttpGet]
        [Route("verifyemail")]
        public ContentResult GenerateVerifyEmailPage([FromQuery][Required] string token, [FromQuery][Required] string id)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>Verify Your Email</title></head><body style=\"background-color:gray\"><div style=\"position:relative; top: 50%; transform:translateY(50%); -webkit-transform:translateY(50%); background-color:whitesmoke; padding: 50px 100px; width:300px; margin:0 auto; border:3px solid blue;\"><form action=\"");
            sb.Append(GlobalSettings.BASE_URL);
            sb.Append("/api/identity/verifyemail?token=");
            sb.Append(token);
            sb.Append("&id=");
            sb.Append(id);
            sb.Append("\" method=\"POST\" style=\"display:block; margin:40px auto 0;\"><h2 style=\"text-align:center;\">Yet Another Library App</h2><button type=\"submit\" style=\"margin:auto; display:block; background-color:blue; color:white; padding:10px 20px; border:1px solid blue; border-radius:5px;\">Verify Email</button></form></div></body></html>");

            return Content(sb.ToString(), "text/html");
        }

        [HttpPost]
        [Route("verifyemail")]
        public async Task<ContentResult> VerifyEmail([FromQuery][Required] string token, [FromQuery][Required] string id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if (user == null) return Content(BuildHtmlMessageWindow("Account not found"), "text/html");

            if (user.EmailConfirmed) return Content(BuildHtmlMessageWindow("Email already verified"), "text/html");

            var result = await userManager.ConfirmEmailAsync(user, Base64Decode(token));
            if (!result.Succeeded) return Content(BuildHtmlMessageWindow("Invalid Code"), "text/html");
            return Content(BuildHtmlMessageWindow("Success"), "text/html");
        }

        [HttpPost]
        [Route("requestreset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
        {
            string email = request.Email.Trim().ToLower();

            ApplicationUser user = await userManager.FindByEmailAsync(email);
            if (user == null) return Ok();

            var resetCode = await userManager.GeneratePasswordResetTokenAsync(user);

            emailService.SendEmail(user.Email, "Password Reset",
                @"Use the below code to reset your account password in the app.

Do not share this code with anyone else. It will expire after 15 minutes.

" + resetCode);

            return Ok();
        }

        [HttpPost]
        [Route("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetRequest request)
        {
            string email = request.Email.Trim().ToLower();

            ApplicationUser user = await userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("Account not found");

            try
            {
                var result = await userManager.ResetPasswordAsync(user, request.ResetCode, request.Password);
                if (result.Succeeded) return Ok();
            }
            catch (Exception) { }

            return BadRequest("Invalid reset code");
        }

        [HttpGet]
        [Route("testAuth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            return Ok();
        }

        private string BuildHtmlMessageWindow(string msg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>Verify Your Email</title></head><body style=\"background-color:gray\"><div style=\"position:relative; top: 50%; transform:translateY(50%); -webkit-transform:translateY(50%); background-color:whitesmoke; padding: 50px 100px; width:300px; margin:0 auto; border:3px solid blue;\"><h2 style=\"text-align:center;\">");
            sb.Append(msg);
            sb.Append("</h2></form></div></body></html>");

            return sb.ToString();
        }

        private string Base64Encode(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        private string Base64Decode(string str)
        {
            var bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
