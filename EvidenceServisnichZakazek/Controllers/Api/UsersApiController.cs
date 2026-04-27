using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using EvidenceServisnichZakazek.Repositories;
using Microsoft.AspNetCore.Mvc;
using EvidenceServisnichZakazek.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace EvidenceServisnichZakazek.Controllers.Api;

[ApiController]
[Route("api/users")]
public class UsersApiController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    
    public UsersApiController(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userRepository.DeleteUserAsync(id);
        return Ok();
    }

    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("email is required");
        }
        
        var users = await _userRepository.GetUserByEmailAsync(email.ToLower());
        
        return Ok(new{ isTaken = users != null });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel loginData) //metoda pro generace JWT tokenu
    {
        var user = await _userRepository.GetUserByEmailAsync(loginData.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginData.Password, user.PasswordHash))
        {
            return Unauthorized("Wrong password or email!");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Admin"),
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken
        (
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );
        
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new {Token = tokenString});
    }

    [Authorize]
    [HttpPost("secret-data")]
    public IActionResult GetSecretData()
    {
        return Ok(new { message="Welcome to VIP zone! Here is your VIP token."});
    }
}

[Route("api/docs")]
public class DocsController : Controller
{
    [HttpGet]
    public IActionResult GetDocs()
    {
        var currProject = Assembly.GetExecutingAssembly();

        var controllers = currProject.GetTypes()
            .Where(type => type.Name.EndsWith("Controller") && !type.IsAbstract);

        var apiDoc = new List<object>();

        foreach (var controller in controllers)
        {
            var methods =  controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var getHttpAtribute = method.CustomAttributes.FirstOrDefault(attr =>
                    attr.AttributeType.Name.StartsWith("Http"));

                if (getHttpAtribute == null) continue;
                
                string httpMethod = getHttpAtribute.AttributeType.Name
                    .Replace("Http", "")
                    .Replace("Attribute", "")
                    .ToUpper();
                
                var methodParameters = method.GetParameters().Select(p => new
                {
                    ParameterName = p.Name,
                    DataType = p.ParameterType.Name
                }).ToList();
                
                apiDoc.Add(new
                {
                    endPoint = $"{controller.Name.Replace("Controller","")}/{method.Name}",
                    httpType = httpMethod,
                    returns = method.ReturnType.Name,
                    param = methodParameters    
                });
            }
        }

        return Json(apiDoc);
    }
}
