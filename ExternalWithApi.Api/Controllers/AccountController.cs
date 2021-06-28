using ExternalWithApi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExternalWithApi.Api.Controllers
{
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost("user/add")]
        public async Task<IActionResult> AddNew([FromBody] UserViewModel addedUser)
        {
            var cheackUser = await userManager.FindByEmailAsync(addedUser.Email);
            if (cheackUser != null)
                return BadRequest("user exists");

            var user = new User { UserName = addedUser.Email, Email = addedUser.Email };
            var result = await userManager.CreateAsync(user, addedUser.Password);
            if(result.Succeeded)
            {
                return Ok("user is added");
            }

            return BadRequest("user isn't added");
        }

        [HttpPost("user/login")]
        public async Task<IActionResult> Login([FromBody] UserViewModel loginUser)
        {
            var user = await userManager.FindByEmailAsync(loginUser.Email);
            if (user == null)
                return BadRequest("user not found");

            var result = userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, loginUser.Password);
            if (result == PasswordVerificationResult.Failed)
                return BadRequest(new { Message = "Login failed" });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return Ok(new { Message = "You are logged in" });
        }
    }
}
