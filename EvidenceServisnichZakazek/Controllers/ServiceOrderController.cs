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

    // Отдает пустую форму клиенту
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Ловит заполненную форму после нажатия кнопки "Отправить"
    [HttpPost]
    public async Task<IActionResult> Create(CreateServiceOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Достаем ID текущего клиента прямо из его Cookies!
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized(); // На всякий случай
            
        int currentUserId = int.Parse(userIdClaim.Value);

        // Собираем заказ для базы данных
        var newOrder = new ServiceOrders
        {
            CustomerId = currentUserId,
            // Склеиваем категорию и модель в одну строку для базы (например: "Смартфон - iPhone 13")
            PhoneType = $"{model.DeviceCategory} - {model.DeviceModel}", 
            IssueDescription = model.IssueDescription,
            CreatedAt = DateTime.Now
        };

        // Сохраняем заказ в БД (твой репозиторий сам запишет и заказ, и историю!)
        await _orderRepository.CreateOrderAsync(newOrder);

        // Пока перекидываем на главную (потом заменим на страницу "Мои заказы")
        return RedirectToAction("Index", "Home"); 
    }

}