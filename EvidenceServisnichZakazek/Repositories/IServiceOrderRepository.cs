using EvidenceServisnichZakazek.Models;

namespace EvidenceServisnichZakazek.Repositories;

public interface IServiceOrderRepository
{
    Task<int> CreateOrderAsync(ServiceOrders order);
}