using QRCoder;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ModulesRegistry.Data.Extensions;

public static class QRCodeExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static string QRCode<TItem>(this TItem item, Func<TItem, object> data)
    {
        var png = AsPng(data(item));
        return string.Format("data:image/pgn;base64,{0}", Convert.ToBase64String(png));
    }

    private static byte[] AsPng(this object data)
    {
        var json = JsonSerializer.Serialize(data, Options);
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(json, QRCodeGenerator.ECCLevel.L);
        using var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(10);
    }
  
}
