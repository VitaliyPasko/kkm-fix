using Dapper;
using FixKkmApp.Models;
using Npgsql;

namespace FixKkmApp;

public class Repository
{
    private const string ConnectionString =
        "Host=hscfiscal-db.c5hhtppidctc.us-west-2.rds.amazonaws.com;Port=5432;Database=HSCFiscal;Username=sa;Password=Ask78sdf!;";

    public async Task<Ticket?> GetTicketAsync(string ticketId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        string query = @"
        SELECT 
            t.""KkmId"", 
            t.""Id"", 
            t.""ShiftId"", 
            t.""CreationDateTime"", 
            t.""FiscalNumber"", 
            t.""QrCode"", 
            t.""BillId"",
            b.""Receipt""
        FROM ""TicketOperations"" t
        JOIN ""Bills"" b ON t.""BillId"" = b.""Id""
        WHERE t.""Id"" = @id
    ";
        var ticket = await connection.QueryFirstOrDefaultAsync<Ticket>(query, new { Id = ticketId });

        return ticket;
    }

}