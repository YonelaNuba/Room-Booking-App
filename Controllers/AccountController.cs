
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoomBookingSystem.Data;

namespace RoomBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> sim, UserManager<ApplicationUser> um)
        { 
            _signInManager = sim; 
            _userManager = um;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null) => View(model: returnUrl);

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, true, lockoutOnFailure: false);
                if (result.Succeeded) return Redirect(returnUrl ?? "/");
            }
            ViewBag.Error = "Invalid credentials";
            return View(model: returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: true);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = string.Join("; ", result.Errors.Select(e => e.Description));
            return View();
        }
    }
}
