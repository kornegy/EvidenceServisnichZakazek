using EvidenceServisnichZakazek.Models;
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
        var orders = await _orderRepository.GetAllOrdersAsync();
        
        return Ok(orders);
    }
    
    [HttpPut("update")]
    public async Task<IActionResult> UpdateOrder([FromBody] ServiceOrders updatedOrder)
    {
        if (updatedOrder == null || updatedOrder.Id == 0)
        {
            return BadRequest("Incorrect orders data");
        }
        
        await _orderRepository.UpdateOrderAsync(updatedOrder);
        
        return Ok(new { message = "Order has been successfully updated!" });
    }

    [HttpDelete("admin/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        bool isDeleted = await _orderRepository.DeleteOrdersAdminAsync(id);

        if (isDeleted)
        {
            return Ok();
        }
        
        return NotFound("Order has not been found!");
    }
    
    [HttpGet("admin/stats")]
    public async Task<IActionResult> GetAppStatistics()
    {
        var stats = await _orderRepository.GetStatisticsAsync();
        return Ok(stats);
    }
    
    [HttpGet("technicians/available/{orderId}")]
    public async Task<IActionResult> GetAvailableTechnicians(int orderId)
    {
        var techs = await _orderRepository.GetAvailableTechniciansAsync(orderId);
        return Ok(techs);
    }
}