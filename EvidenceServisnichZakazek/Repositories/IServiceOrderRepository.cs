using EvidenceServisnichZakazek.Models;

namespace EvidenceServisnichZakazek.Repositories;

public interface IServiceOrderRepository
{
    Task<int> CreateOrderAsync(ServiceOrders order);
    Task<IEnumerable<ServiceOrders>> GetOrdersByUserIdAsync(int userId);
    Task<bool> DeleteOrderAsync(int orderId, int customerId);
}