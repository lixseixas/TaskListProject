using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
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

            var client = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["TaskReportApi:Url"] ?? "https://localhost:44322";
            
            var loginRequest = new
            {
                User = userModel.User,
                Password = userModel.Password
            };

            var jsonContent = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{apiUrl}/api/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(userModel);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var token = loginResponse.GetProperty("token").GetString();

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

                // Redirect to Welcome on successful login
                return RedirectToAction(nameof(Welcome));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling TaskReportApi for login");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(userModel);
            }
        }
        [AllowAnonymous]
        public IActionResult Welcome()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewBag.Username = User.Identity.Name;
                ViewBag.Message = "You have successfully logged in.";
                ViewBag.IsLoggedIn = true;
            }
            else if (TempData["LoggedOutUser"] != null)
            {
                ViewBag.Username = TempData["LoggedOutUser"];
                ViewBag.Message = "You have successfully logged out.";
                ViewBag.IsLoggedIn = false;
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["LoggedOutUser"] = username;
            return RedirectToAction(nameof(Welcome));
        }
                
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
