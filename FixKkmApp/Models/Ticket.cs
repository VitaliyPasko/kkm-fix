namespace FixKkmApp.Models;

public class Ticket
{
    public string Id { get; set; }
    public string KkmId { get; set; }
    public string ShiftId { get; set; }
    public DateTime CreationDateTime { get; set; }
    public string FiscalNumber { get; set; }
    public string QrCode { get; set; }
    public string BillId { get; set; }
    public string Receipt { get; set; }
}