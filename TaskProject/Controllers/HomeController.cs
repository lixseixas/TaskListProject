using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TaskProject.Bl;
using TaskProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TaskProject.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TaskContext _context;

        public HomeController(ILogger<HomeController> logger, TaskContext context)
        {
            _logger = logger;
            _context = context;
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
          
            TasksDal taskDb = new TasksDal();
            var token = taskDb.GetUserPassword(userModel.User, userModel.Password);

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
