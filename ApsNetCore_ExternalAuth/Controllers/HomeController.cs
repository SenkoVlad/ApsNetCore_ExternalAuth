using ApsNetCore_ExternalAuth.Infrastructure.Data;
using ApsNetCore_ExternalAuth.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApsNetCore_ExternalAuth.Controllers
{
    public class HomeController : Controller
    {
        protected SignInManager<User> _signInManager { get; set; }
        protected UserManager<User> _userManager { get; set; }
        public HomeController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Admin()
        {
            var claims = HttpContext.User;
            return View(claims);
        }

        public IActionResult ExternalLogin(string returnUrl = null) 
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Home", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> ExternalLoginCallback(string redirectUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                return Redirect(nameof(Admin));
            }
            if (signInResult.IsLockedOut)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewData["ReturnUrl"] = redirectUrl;
                ViewData["Provider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginModel { Email = email });
            }
        }

        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return View(nameof(Index));
            var user = await _userManager.FindByEmailAsync(model.Email);
            IdentityResult result;
            if (user != null)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Redirect(nameof(Admin));
                }
            }
            else
            {
                model.Principal = info.Principal;
                user = new User { Email = model.Email, UserName = model.Email };
                result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return Redirect(nameof(Admin));
                    }
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }
            return View(nameof(ExternalLogin), model);
        }
    }
}
