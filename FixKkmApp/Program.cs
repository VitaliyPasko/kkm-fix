
using System.Globalization;
using System.Web;
using FixKkmApp;
Console.OutputEncoding = System.Text.Encoding.UTF8;
try
{
    // Repository repository = new Repository();
    // Ticket? ticket = await repository.GetTicketAsync("c25498cb-b558-4940-ac1d-cefb6ab5d8de");
    // Console.WriteLine(ticket?.FiscalNumber);
    Repository repository = new Repository();
    var ticket = await repository.GetTicketAsync("86c12ed3-c021-4260-b3a0-a24d38d34a7f");
    if (ticket == null)
        throw new ApplicationException("Ticket not found");
    
    QrGenerator qrGenerator = new QrGenerator();
    
    var oldReceipt = ticket.Receipt;
    var link = ticket.QrCode;
    var newCretedDate = DateTime.Now;
    var newFiscalNumber = ticket.FiscalNumber;
    
    
    decimal? newAmount = null;
    link = GetNewLink(
        link, 
        newFiscalNumber: newFiscalNumber, 
        newS: newAmount.Value.ToString("0.00", CultureInfo.InvariantCulture), 
        newT: newCretedDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture));
    
    var newBase64 = qrGenerator.Generate(link);
    Console.WriteLine(newBase64);

    Recept recept = new Recept();
    var newReceipt = recept.Update(
        json: oldReceipt,
        newFiscalNumber: newFiscalNumber,
        newCreationDate: newCretedDate,
        newbase64: newBase64,
        newLink: link,
        newPrice: newAmount);
    Console.WriteLine(newReceipt);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine(e.StackTrace);
}

static string GetNewLink(string link, 
    string? newFiscalNumber = null,
    string? newF = null,
    string? newS = null, 
    string? newT = null)
{
    var uri = new Uri(link);
    var query = HttpUtility.ParseQueryString(uri.Query);
    
    // Обновляем параметры в строке запроса
    
    query["i"] = newFiscalNumber ?? query["i"];
    query["f"] = newF ?? query["f"];
    query["s"] = newS ?? query["s"];
    query["t"] = newT ?? query["t"];

    // Формируем новый URI с обновленными параметрами
    var updatedUri = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}?{query}";

    return updatedUri;
}