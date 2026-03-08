using Dapper;
using Microsoft.Data.Sqlite;

namespace EvidenceServisnichZakazek.Data
{

    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql = @"

                CREATE TABLE IF NOT EXISTS Users
                (
                   Id INTEGER PRIMARY KEY AUTOINCREMENT,
                   Email text not null,
                   FullName text not null,
                   PasswordHash text not null,
                   phoneNumber text not null
                );
                
                CREATE TABLE IF NOT EXISTS Technicians
                (
                    Id INTEGER primary key AUTOINCREMENT,
                    Name text not null
                );

                CREATE TABLE IF NOT EXISTS ServiceOrders
                (
                    Id INTEGER primary key AUTOINCREMENT,
                    CustomerId INTEGER not null,
                    TechniciansId INTEGER null,
                    PhoneType text not null,
                    IssueDescription text not null,
                    Price real null,
                    CurrStatus INTEGER not null default (1),
                    CreatedAt text not null,
                    Foreign Key(CustomerId) References Users(Id),
                    Foreign Key(TechniciansId) References Technicians(Id)
                );

                CREATE TABLE IF NOT EXISTS OrderHistories
                (
                    Id INTEGER primary key AUTOINCREMENT,
                    OrderId INTEGER not null,
                    Status INTEGER not null,
                    ChangedAt text not null,
                    DurationMinutes INTEGER not null,
                    Foreign Key(OrderId) References ServiceOrders(Id)
                );
            ";
            
            connection.Execute(sql);
        }
    }
}