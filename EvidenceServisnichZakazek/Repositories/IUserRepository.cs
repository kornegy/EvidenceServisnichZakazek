using EvidenceServisnichZakazek.Models;

namespace EvidenceServisnichZakazek.Repositories;

public interface IUserRepository
{
    Task<int> AddUserAsync(Users users);
    Task<Users?> GetUserByEmailAsync (string email);
}