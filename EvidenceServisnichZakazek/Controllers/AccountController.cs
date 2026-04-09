using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using EvidenceServisnichZakazek.Repositories;

namespace EvidenceServisnichZakazek.Controllers;
using Microsoft.AspNetCore.Mvc;
using EvidenceServisnichZakazek.Models;
using EvidenceServisnichZakazek.Repositories;
using BCrypt.Net;

public class AccountController : Controller
{
    private readonly IUserRepository _userRepository;
    
    public AccountController(IUserRepository userRepository)
    {
        this._userRepository =  userRepository;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel registerModel)
    {
        if (!ModelState.IsValid)
        {
            return View(registerModel);
        }
        
        var existingUser = await _userRepository.GetUserByEmailAsync(registerModel.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View(registerModel);
        }
        
        //password hashing proccess
        string hashedPassword = BCrypt.HashPassword(registerModel.Password);

        var newUser = new Users
        {
            Email = registerModel.Email,
            FullName = registerModel.FullName,
            PhoneNumber = registerModel.PhoneNumber,
            PasswordHash = hashedPassword,
        };
        
        newUser.Id = await _userRepository.AddUserAsync(newUser);
        
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, newUser.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, newUser.FullName),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, newUser.Email)
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, 
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            
        await HttpContext.SignInAsync(
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, 
            new System.Security.Claims.ClaimsPrincipal(identity));

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login (LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        string safeEmail = model.Email.ToLower();
        
        var user = await _userRepository.GetUserByEmailAsync(safeEmail);

        if (user == null || !BCrypt.Verify(model.Password, user.PasswordHash)) //porovnava heslo vedene uzivatele z cashem v DB
        {
            ModelState.AddModelError("", "Wrong email or password!");
            return View(model);
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email)
        };
        
        var identity = new System.Security.Claims.ClaimsIdentity(claims,
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme); //vytvorime pass a sifrujeme ho v cookeis
        
        await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, 
            new System.Security.Claims.ClaimsPrincipal(identity));

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null) return RedirectToAction("Login");
        
        int currentIdUser = int.Parse(userIdClaim.Value);
        
        var currentUser = await _userRepository.GetUserByIdAsync(currentIdUser);
        return View(currentUser);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(string newFullName, string newPhoneNumber)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        int currentUserId = int.Parse(userIdClaim.Value);

        bool success = await _userRepository.UpdateUserProfileAsync(currentUserId, newFullName);

        if (success == true) //pousiti AJAX
        {
            return Json(new { success = true, message = "Changes has been successfully updated" });
        }
        else
        {
            return Json(new { success = false, message = "Error while updating" });
        }
    }
    
    
}