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

    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var existingUser = await _userRepository.GetUserByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email| ", "Email already exists");
            return View(model);
        }
        
        //password hashing proccess
        string hashedPassword = BCrypt.HashPassword(model.Password);

        var newUser = new Users
        {
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            PasswordHash = hashedPassword,
        };
        
        await _userRepository.AddUserAsync(newUser);
        
        return RedirectToAction("Index", "Home");
    }
    
}