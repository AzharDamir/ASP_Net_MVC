using Microsoft.AspNetCore.Mvc;
using MovieStore.Models.DTO;
using MovieStore.Repositories.Abstract;

namespace MovieStore.Controllers
{
    public class UserAuthenticationController : Controller
    {
        private IUserAuthenticationServices authService;
        public UserAuthenticationController(IUserAuthenticationServices authServices) {
            this.authService = authServices;
         
        }
        public async Task<IActionResult> Register()
        {
            var modelUser = new Registration
            {
                Email = "user@gmail.com",
                Username = "user",
                Name = "user",
                Password = "User@123",
                PasswordConfirm = "User@123",
                Role = "User"
            };
            var modelAdmin = new Registration
            {
                Email = "admin@gmail.com",
                Username = "Admin",
                Name = "admin",
                Password = "Admin@123",
                PasswordConfirm = "Admin@123",
                Role = "Admin"
            };
            var result1=await authService.RegisterAsync(modelUser);
            var result = await authService.RegisterAsync(modelAdmin);
            return Ok(result.Message+result1.Message);
        }
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await authService.LoginAsync(model);
            if (result.StatusCode == 1)
                return RedirectToAction("Index", "Home");
            else
            {
                TempData["msg"] = "Could not logged in..";
                return RedirectToAction(nameof(Login));
            }
        }

        public async Task<IActionResult> Logout()
        {
            await authService.LogoutAsync();
            return RedirectToAction(nameof(Login));
        }

    }

}
