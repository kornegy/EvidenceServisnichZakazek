using System.Reflection;
using EvidenceServisnichZakazek.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceServisnichZakazek.Controllers.Api;

[ApiController]
[Route("api/users")]
public class UsersApiController : Controller
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