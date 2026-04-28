using System.Data;
using System.Data.Common;
using System.Text;
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

        string insertOrderSql =
            @"INSERT INTO ServiceOrders (CustomerId, PhoneType, IssueDescription, CreatedAt, CurrStatus)
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

    public async Task<IEnumerable<ServiceOrders>> GetOrdersByUserIdAsync(int userId)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        string sql = @"SELECT * FROM ServiceOrders WHERE CustomerId = @UserId ORDER BY CreatedAt DESC";

        return await db.QueryAsync<ServiceOrders>(sql, new { UserId = userId });
    }

    public async Task<bool> DeleteOrderAsync(int orderId, int customerId)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        string checksql =
            "SELECT Id FROM ServiceOrders Where Id = @OrderId AND @CustomerId = CustomerId AND CurrStatus = 1";

        var id = await db.ExecuteScalarAsync<int?>(checksql, new { OrderId = orderId, CustomerId = customerId });

        if (id == null)
            return false;

        string deletesql = @"DELETE FROM OrderHistories WHERE OrderId = @OrderId;
                             DELETE FROM ServiceOrders WHERE Id = @OrderId;
        ";

        await db.ExecuteAsync(deletesql, new { OrderId = orderId });
        return true;
    }

    public async Task<IEnumerable<ServiceOrders>> GetUserOrdersFilteredAsync(int userId, string searchQuery,
        int? statusFilter, string dateFrom)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        var sql = new StringBuilder("Select * FROM ServiceOrders where customerId = @userId");
        var parameters = new DynamicParameters();
        parameters.Add("@userId", userId);

        if (!string.IsNullOrEmpty(searchQuery))
        {
            sql.Append(" and (PhoneType LIKE @search or IssueDescription LIKE @search)");
            parameters.Add("@search", $"%{searchQuery}%");
        }

        if (statusFilter.HasValue)
        {
            sql.Append(" and CurrStatus = @status");
            parameters.Add("@status", statusFilter.Value);
        }

        if (!string.IsNullOrEmpty(dateFrom))
        {
            sql.Append(" and CreatedAt >= @dateFrom");
            parameters.Add("@dateFrom", dateFrom);
        }

        sql.Append(" ORDER BY CreatedAt DESC");

        return await db.QueryAsync<ServiceOrders>(sql.ToString(), parameters);
    }

    public async Task<IEnumerable<ServiceOrders>> GetAllOrdersAsync()
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        string sql = "Select * From ServiceOrders ORDER BY CreatedAt DESC";
        
        return await db.QueryAsync<ServiceOrders>(sql);
    }

    public async Task UpdateOrderAsync(ServiceOrders order)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);
        db.Open();
        using var transaction = db.BeginTransaction();
        
        try
        {
            var currentData = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT CurrStatus, CreatedAt FROM ServiceOrders WHERE Id = @Id",
                new { Id = order.Id });

            if (currentData == null) return;

            int oldStatus = Convert.ToInt32(currentData.CurrStatus);

            if (order.TechniciansId == 0)
            {
                order.TechniciansId = null;
            }

            string sql = @"UPDATE ServiceOrders 
                       SET PhoneType = @PhoneType, 
                           IssueDescription = @IssueDescription,
                           Price = @Price,
                           CurrStatus = @CurrStatus,
                           TechniciansId = @TechniciansId 
                       WHERE Id = @Id";

            await db.ExecuteAsync(sql, order);

            if (oldStatus != order.CurrStatus)
            {
                string lastChangeStr = await db.QueryFirstOrDefaultAsync<string>(
                    "SELECT ChangedAt FROM OrderHistories WHERE OrderId = @OrderId ORDER BY Id DESC LIMIT 1",
                    new { OrderId = order.Id });

                DateTime lastChange;

                if (!string.IsNullOrEmpty(lastChangeStr))
                {
                    lastChange = DateTime.Parse(lastChangeStr);
                }
                else
                {
                    lastChange = DateTime.Parse((string)currentData.CreatedAt);
                }

                int durationMinutes = (int)(DateTime.Now - lastChange).TotalMinutes;

                string sqlHistory =
                    @"INSERT INTO OrderHistories (OrderId, Status, ChangedAt, DurationMinutes) VALUES (@OrderId, @Status, @ChangedAt, @DurationMinutes)";

                await db.ExecuteAsync(sqlHistory, new
                {
                    OrderId = order.Id,
                    Status = order.CurrStatus,
                    ChangedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    DurationMinutes = durationMinutes
                }, transaction);
            }

            transaction.Commit();
        }
        catch (Exception) 
        {
            transaction.Rollback();
            throw;
        }
    }
    
    public async Task<bool> DeleteOrdersAdminAsync(int orderId)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);

        string deleteHistorySql = "DELETE FROM OrderHistories WHERE OrderId = @Id";
        await db.ExecuteAsync(deleteHistorySql, new { Id = orderId });
        
        string deleteOrderSql = "DELETE FROM ServiceOrders WHERE Id = @Id";
        int rowsAffected = await db.ExecuteAsync(deleteOrderSql, new { Id = orderId });
        
        
        return rowsAffected > 0;
    }
    
    public async Task<StatisticsDTO.AppStatisticsDto> GetStatisticsAsync()
    {
        using IDbConnection db = new SqliteConnection(_connectionString);
        var stats = new StatisticsDTO.AppStatisticsDto();
        
        stats.TotalOrders = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ServiceOrders");
        
        string sqlStats = @"
        SELECT Status, 
               COUNT(*) as TimesVisited, 
               AVG(DurationMinutes) as AvgMinutes 
        FROM OrderHistories 
        GROUP BY Status";

        var statusData = await db.QueryAsync<StatisticsDTO.StatusStatDto>(sqlStats);
        stats.StatusStats = statusData.ToList();

        return stats;
    }
    
    public async Task<IEnumerable<TechnicianDTO>> GetAvailableTechniciansAsync(int currentOrderId)
    {
        using IDbConnection db = new SqliteConnection(_connectionString);
        
        string sql = @"
        SELECT Id, Name 
        FROM Technicians 
        WHERE Id NOT IN (
            SELECT TechniciansId 
            FROM ServiceOrders 
            WHERE TechniciansId IS NOT NULL 
              AND CurrStatus IN (1, 2) 
              AND Id != @OrderId
        )";

        return await db.QueryAsync<TechnicianDTO>(sql, new { OrderId = currentOrderId });
    }
}