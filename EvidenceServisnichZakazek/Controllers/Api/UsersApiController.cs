using EvidenceServisnichZakazek.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceServisnichZakazek.Controllers.Api;

[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly IUserRepository userRepository;
    
    public UsersApiController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("email is required");
        }
        
        var users = await userRepository.GetUserByEmailAsync(email.ToLower());
        
        return Ok(new{ isTaken = users != null });
    }
}