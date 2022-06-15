using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using LoginAndReg.Models;
using Microsoft.AspNetCore.Http;

namespace LoginAndReg.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private UserContext _context;

    public HomeController(ILogger<HomeController> logger, UserContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Home()
    {
        int? Id = HttpContext.Session.GetInt32("UserId");
        if (Id != null)
        {
            return RedirectToAction("Success");
        }
        return View("Home");
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        int? Id = HttpContext.Session.GetInt32("UserId");
        if (Id != null)
        {
            return RedirectToAction("Success");
        }
        return View("Login");
    }

    [HttpGet("/success")]
    public ViewResult Success()
    {
        return View("Success");
    }

    [HttpGet("/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Home");
    }

    [HttpPost("user/create")]
    public IActionResult CreateUser(User newUser)
    {
        if (ModelState.IsValid)
        {
            if (_context.Users.Any(user => user.Email == newUser.Email))
            {
                ModelState.AddModelError("Email", "Email already in use");
                return View("Login");
            }
            else
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Add(newUser);
                _context.SaveChanges();
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == newUser.Email);
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("Success");
            }
        }
        return View("Login");
    }

    [HttpPost("user/login")]
    public IActionResult LoginUser(LoginUser user)
    {
        if (ModelState.IsValid)
        {
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userInDb == null)
            {
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("Login");
            }
            var hasher = new PasswordHasher<LoginUser>();
            var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.Password);
            if (result == 0)
            {
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("Login");
            }
            HttpContext.Session.SetInt32("UserId", userInDb.UserId);
            return RedirectToAction("Success");
        }
        return View("Login");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
