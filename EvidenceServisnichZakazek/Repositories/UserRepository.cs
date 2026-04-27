using Dapper;
using System.Data;
using Microsoft.Data.Sqlite;
using EvidenceServisnichZakazek.Models;

namespace EvidenceServisnichZakazek.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<Users?> GetUserByIdAsync(int id)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);
        return await db.QueryFirstAsync<Users>($"SELECT * FROM Users WHERE Id = {id}", new { id });
    }

    public async Task<int> AddUserAsync(Users users) // pridani noveho uzivatele v DB
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        string sql = @"
                INSERT INTO Users(Email, FullName, PasswordHash, PhoneNumber)
                VALUES (@Email, @FullName, @PasswordHash, @PhoneNumber);
        
                SELECT last_insert_rowid(); ";
        
        return await db.ExecuteScalarAsync<int>(sql, users);
    }

    public async Task<Users?> GetUserByEmailAsync(string email)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        string sql = @"
            SELECT *
            FROM Users
            WHERE Email = @Email LIMIT 1;
            ";
        
        return await db.QuerySingleOrDefaultAsync<Users>(sql, new { Email = email }); //vrati null, jest-li email neexistuje
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, string newFullName)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        string updatesql = "UPDATE Users SET FullName = @newFullName Where Id = @userId";

        return await db.ExecuteAsync(updatesql, new { newFullName = newFullName, userId = userId }) > 0;
    }
    
    public async Task<IEnumerable<UsersDTO>> GetAllUsersAsync()
    {
        using IDbConnection db = new SqliteConnection(_connectionString);
        return await db.QueryAsync<UsersDTO>("SELECT Id, FullName, Email, PhoneNumber FROM Users");
    }

    public async Task DeleteUserAsync(int id)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        await db.ExecuteAsync("DELETE FROM Users WHERE Id = @Id", new { Id = id });
    }
}