using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using EvidenceServisnichZakazek.Models;

namespace EvidenceServisnichZakazek.Repositories;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly string _connectionString;
    
    public ServiceOrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<int> CreateOrderAsync(ServiceOrders order)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        string insertOrderSql = @"INSERT INTO ServiceOrders (CustomerId, PhoneType, IssueDescription, CreatedAt, CurrStatus)
                VALUES (@CustomerId, @PhoneType, @IssueDescription, @CreatedAt, 1);
                
                SELECT last_insert_rowid();";
        
        int newOrderId = await db.ExecuteScalarAsync<int>(insertOrderSql, order);

        string insertHistorySql = @"INSERT INTO OrderHistories (OrderId, Status, ChangedAt, DurationMinutes)
                VALUES (@OrderId, 1, @ChangedAt, 0);";
        
        await db.ExecuteAsync(insertHistorySql, new 
        {
            OrderId = newOrderId, 
            ChangedAt = order.CreatedAt 
        });

        return newOrderId;
    }
}