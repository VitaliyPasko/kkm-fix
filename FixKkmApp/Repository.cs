using System.Data;
using Dapper;
using FixKkmApp.Models;
using Npgsql;

namespace FixKkmApp;

public class Repository
{
    private const string ConnectionString =
        // "Host=hscfiscal-db.c5hhtppidctc.us-west-2.rds.amazonaws.com;Port=5432;Database=HSCFiscal;Username=sa;Password=Ask78sdf!;";
        "Host=localhost;Port=5432;Database=posdbtest2;Username=postgres;Password=postgres;";

    public async Task<Ticket?> GetTicketAsync(string ticketId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        string query = $"""
                            SELECT 
                                t."KkmId", 
                                t."Id", 
                                t."ShiftId", 
                                t."CreationDateTime", 
                                t."FiscalNumber", 
                                t."QrCode", 
                                t."BillId",
                                b."Receipt"
                            FROM "TicketOperations" t
                            JOIN "Bills" b ON t."BillId" = b."Id"
                            WHERE t."Id" = @id
                        """;
        var ticket = await connection.QueryFirstOrDefaultAsync<Ticket>(query, new { Id = ticketId });

        return ticket;
    }

    public async Task UpdateTicket(Ticket ticket)
    {
        try
        {
            using (IDbConnection db = new NpgsqlConnection(ConnectionString))
            {
                db.Open();

                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // Вставка
                        string createBill = $"""
                                                 INSERT INTO "Bills" ("Id", "CreationDateTime", "UpdateDateTime", "Receipt")
                                                 VALUES (@BillId, @CreationDateTime, @UpdateDateTime, @Receipt);
                                             """;

                        int rowsAffected = await db.ExecuteAsync(createBill,
                            new
                            {
                                BillId = ticket.BillId,
                                CreationDateTime = ticket.CreationDateTime,
                                UpdateDateTime = "0001-01-01 00:00:00.000000",
                                Receipt = ticket.Receipt
                            },
                            transaction);

                        if (rowsAffected == 0)
                        {
                            throw new Exception("Не было вставлено ни одной завписи");
                        }

                        // Обновление
                        string insertQuery = $"""
                                              UPDATE "TicketOperations"
                                              SET "BillId" = @BillId,
                                                  "ShiftId" = @ShiftId
                                              WHERE "Id"= @Id;
                                              """;

                        await db.ExecuteAsync(insertQuery,
                            new
                            {
                                BillId = ticket.BillId,
                                ShiftId = ticket.ShiftId,
                                Id = ticket.Id,
                            },
                            transaction);

                        // Фиксируем транзакцию
                        transaction.Commit();
                        Console.WriteLine("Транзакция успешно выполнена!");
                    }
                    catch (Exception ex)
                    {
                        // Откат транзакции в случае ошибки
                        transaction.Rollback();
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка соединения: {ex.Message}");
        }
    }
}