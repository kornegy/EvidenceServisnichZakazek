using EvidenceServisnichZakazek.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceServisnichZakazek.Controllers.Api;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/orders")]
[ApiController]
public class OrdersApiController : Controller
{
    private readonly IServiceOrderRepository _orderRepository;
    
    public OrdersApiController(IServiceOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = _orderRepository.GetAllOrdersAsync();
        
        return Ok(orders);
    }
}