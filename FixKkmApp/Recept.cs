using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FixKkmApp;

public class Recept
{
    public string Update(string json)
    {
        // Парсим JSON в объект
        JObject obj = JObject.Parse(json);

        // 1. Изменение поля "center": "<#FiscalNumber#>/<%FiscalNumber%>:"
        UpdateField(obj, "center", "<#FiscalNumber#>/<%FiscalNumber%>:", "NewFiscalNumber");

        // 2. Изменение поля "center": "<#Time#>/<%Time%>:"
        UpdateField(obj, "center", "<#Time#>/<%Time%>:", "2025-01-01 00:00:00");

        // 3. Изменение base64 в поле "center"
        UpdateBase64Field(obj, "center", "iVBORw0KGgoAAAANSUhEUgAAAJ8AAACfAQAAAADPYDZN");

        // Замена динамических параметров в поле "qrcode"
        UpdateQrCodeField(
            obj, 
            "qrcode", 
            "NewIValue",   // Новый параметр i
            "NewFValue",   // Новый параметр f
            "NewSValue",   // Новый параметр s
            "20250101T000000" // Новый параметр t
        );


        // 5. Изменение поля "dateCreation"
        obj["dateCreation"] = "2025-01-01T00:00:00.000000Z";

        // Вывод измененного JSON
        // Приведение JObject к object для корректного вызова SerializeObject
        string updatedJson = JsonConvert.SerializeObject(obj, Formatting.Indented);

        Console.WriteLine(updatedJson);

        
    }

    static void UpdateField(JObject obj, string key, string matchValue, string newValue)
    {
        var tokens = obj.SelectTokens($"$.rows[?(@.{key} != null)]").ToList();
        foreach (var token in tokens)
        {
            if (token[key]?.ToString().Contains(matchValue) == true)
            {
                token[key] = newValue;
            }
        }
    }
    
    static void UpdateBase64Field(JObject obj, string key, string newValue)
    {
        var base64Pattern = @"^[A-Za-z0-9+/=]+$"; // Простое регулярное выражение для base64
        var tokens = obj.SelectTokens($"$.rows[?(@.{key} != null)]").ToList();

        foreach (var token in tokens)
        {
            string? currentValue = token[key]?.ToString();
            if (!string.IsNullOrEmpty(currentValue) && Regex.IsMatch(currentValue, base64Pattern))
            {
                token[key] = newValue;
            }
        }
    }

    private static void UpdateQrCodeField(JObject obj, string key, string newI, string newF, string newS, string newT)
    {
        var tokens = obj.SelectTokens($"$.rows[?(@.{key} != null)]").ToList();

        foreach (var token in tokens)
        {
            string? currentValue = token[key]?.ToString();
            if (!string.IsNullOrEmpty(currentValue))
            {
                var uri = new Uri(currentValue);
                var query = HttpUtility.ParseQueryString(uri.Query);

                // Обновляем параметры в строке запроса
                query["i"] = newI;
                query["f"] = newF;
                query["s"] = newS;
                query["t"] = newT;

                // Формируем новый URI с обновленными параметрами
                var updatedUri = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}?{query}";
                token[key] = updatedUri;
            }
        }
    }

}
