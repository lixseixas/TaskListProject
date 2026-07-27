using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TaskProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TaskListProject.Infrastructure.Data;

namespace TaskProject.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }               

        [AllowAnonymous]
        public IActionResult UserLogin()
        {
            UserLoginModel taskModel = new UserLoginModel();
            //taskModel.Id = Guid.NewGuid();
            return View(taskModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UserLogin(UserLoginModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userModel);
            }                              
          
            UserQueries userQuery = new UserQueries();
            var token = userQuery.GetUserPassword(userModel.User, userModel.Password);

            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(userModel);
            }

            // Create claims and sign in with cookie authentication so controller actions can use identity.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userModel.User),
                new Claim("JWT", token)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Redirect to Index on successful login
            return RedirectToAction(nameof(Index));
        }
                
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
