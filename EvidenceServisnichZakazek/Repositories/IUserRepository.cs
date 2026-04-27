using EvidenceServisnichZakazek.Models;

namespace EvidenceServisnichZakazek.Repositories;

public interface IUserRepository
{
    Task<int> AddUserAsync(Users users);
    Task<Users?> GetUserByEmailAsync (string email);
    
    Task<bool> UpdateUserProfileAsync(int userId, string newFullName);
    Task<Users?> GetUserByIdAsync(int id);

    Task DeleteUserAsync(int id);
    Task<IEnumerable<UsersDTO>> GetAllUsersAsync();
}