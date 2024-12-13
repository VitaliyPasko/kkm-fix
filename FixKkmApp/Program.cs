
using FixKkmApp;
using FixKkmApp.Models;

try
{
    // Repository repository = new Repository();
    // Ticket? ticket = await repository.GetTicketAsync("c25498cb-b558-4940-ac1d-cefb6ab5d8de");
    // Console.WriteLine(ticket?.FiscalNumber);
    QrGenerator qrGenerator = new QrGenerator();
    var qr = qrGenerator.Generate("https://consumer.kofd.kz?i=796410537055&f=010104024483&s=500.00&t=20241130T232832");
    Console.WriteLine(qr);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine(e.StackTrace);
}