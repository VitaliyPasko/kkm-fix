using QRCoder;


namespace FixKkmApp;

public class QrGenerator
{
    // "https://consumer.kofd.kz?i=772976772853&f=010103412967&s=500.00&t=20240304T181526"
    public string Generate(string link)
    {
        QRCodeGenerator qrCodeGenerator = new();
        QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new(qrCodeData);
        var graphic = qrCode.GetGraphic(3);
        var base64Graphic = Convert.ToBase64String(graphic);
        File.WriteAllText("qr.txt", base64Graphic);
        return base64Graphic;
    }

}