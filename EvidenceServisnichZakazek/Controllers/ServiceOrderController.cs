using System.Security.Claims;
using EvidenceServisnichZakazek.Models;
using EvidenceServisnichZakazek.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceServisnichZakazek.Controllers;

[Authorize]
public class ServiceOrderController : Controller
{
    private readonly IServiceOrderRepository _orderRepository;

    public ServiceOrderController(IServiceOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        
        if(userIdClaim == null) return Unauthorized();

        int currentUserId = int.Parse(userIdClaim.Value);
        
        var orders = await _orderRepository.GetOrdersByUserIdAsync(currentUserId);
        
        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateServiceOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // zjistime id aktualniho clienta
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized(); // Pro pripad
            
        int currentUserId = int.Parse(userIdClaim.Value);

        // sestavujeme objednavku pro db
        var newOrder = new ServiceOrders
        {
            CustomerId = currentUserId,
            PhoneType = $"{model.DeviceCategory} - {model.DeviceModel}", 
            IssueDescription = model.IssueDescription,
            CreatedAt = DateTime.Now
        };

        // commit db
        await _orderRepository.CreateOrderAsync(newOrder);
        
        return RedirectToAction("Index"); 
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        int currentUserId = int.Parse(userIdClaim.Value);

        bool success = await _orderRepository.DeleteOrderAsync(id, currentUserId);

        if (success)
            TempData["SuccesMessage"] = "Orders has been successfully removed";
        else
            TempData["ErrorMessage"] = "Error! The order is already being processed or does not exist";
        
        return RedirectToAction("Index");
    }

}