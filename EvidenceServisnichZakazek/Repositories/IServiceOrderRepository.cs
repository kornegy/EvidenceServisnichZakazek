using EvidenceServisnichZakazek.Models;

namespace EvidenceServisnichZakazek.Repositories;

public interface IServiceOrderRepository
{
    Task<int> CreateOrderAsync(ServiceOrders order);
    Task<IEnumerable<ServiceOrders>> GetOrdersByUserIdAsync(int userId);
    Task<bool> DeleteOrderAsync(int orderId, int customerId);
    
    Task<IEnumerable<ServiceOrders>> GetUserOrdersFilteredAsync(int userId, string searchQuery, int? statusFilter, string dateFrom);

    Task<IEnumerable<ServiceOrders>> GetAllOrdersAsync();
    Task UpdateOrderAsync(ServiceOrders order);
    
    Task<bool> DeleteOrdersAdminAsync(int orderId);
    
    Task<StatisticsDTO.AppStatisticsDto> GetStatisticsAsync();
}