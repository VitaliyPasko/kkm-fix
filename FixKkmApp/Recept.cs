using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FixKkmApp;

public class Recept
{
    public string Update(string json, string? newFiscalNumber = null, DateTime? newCreationDate = null, string? newbase64 = null, string? newLink = null, decimal? newPrice = null)
    {
        // Парсим JSON в объект
        JObject obj = JObject.Parse(json);

        if (!string.IsNullOrWhiteSpace(newFiscalNumber))
        {
            // 1. Изменение поля "center": "<#FiscalNumber#>/<%FiscalNumber%>:"
            UpdateField(obj, "center", newFiscalNumber, "<#FiscalNumber#>/<%FiscalNumber%>:");
        
            // 1. Изменение поля "center": "<#OfflineFiscalNumber#>/<%OfflineFiscalNumber%>:"
            UpdateField(obj, "center", newFiscalNumber, "<#OfflineFiscalNumber#>/<%OfflineFiscalNumber%>:");
        }

        if (newCreationDate.HasValue)
        {
            // 2. Изменение поля "center": "<#Time#>/<%Time%>:" 11/30/2024 23:28:32
            UpdateField(obj, key: "center", newCreationDate.Value.ToString("MM/dd/yyyy HH:mm:ss"), matchValue: "<#Time#>/<%Time%>:");
        
            // "dateCreation": "2024-11-30T23:28:32.153674Z"
            UpdateField(obj, key: "dateCreation", newValue: newCreationDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"));
        }

        if (!string.IsNullOrWhiteSpace(newbase64))
        {
            // 3. Изменение base64 в поле "center"
            UpdateBase64Field(obj, "center", newbase64);
        }

        if (!string.IsNullOrWhiteSpace(newLink))
        {
            // Замена динамических параметров в поле "qrcode"
            UpdateQrCodeField(obj, "qrcode", newLink);
            UpdateField(obj, key: "qrcode", newValue: newLink);

        }

        if (newPrice.HasValue)
        {
            UpdateSumFieldsAdvanced(obj, newPrice.Value);
        }
        
        // Вывод измененного JSON
        // Приведение JObject к object для корректного вызова SerializeObject
        string updatedJson = JsonConvert.SerializeObject(obj, Formatting.Indented);

        Console.WriteLine(updatedJson);
        return string.Empty;

    }

    static void UpdateField(JObject obj, string key, string newValue, string? matchValue = null)
    {
        var tokens = obj.SelectTokens($"$.rows[?(@.{key} != null)]").ToList();
        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(matchValue))
                token[key] = newValue;
            else if (token[key]?.ToString().Contains(matchValue) == true)
                token[key] = newValue;
        }
    }
    
    static void UpdateBase64Field(JObject obj, string key, string newBase64)
    {
        var base64Pattern = @"^[A-Za-z0-9+/=]+$"; // Простое регулярное выражение для base64
        var tokens = obj.SelectTokens($"$.rows[?(@.{key} != null)]").ToList();

        foreach (var token in tokens)
        {
            string? currentValue = token[key]?.ToString();
            if (!string.IsNullOrEmpty(currentValue) && Regex.IsMatch(currentValue, base64Pattern))
            {
                token[key] = newBase64;
            }
        }
    }
    
    private static void UpdateQrCodeField(JObject obj, string key, string newLink)
    {
        var tokens = obj.SelectTokens($"$.rows[?(@.{key} != null)]").ToList();

        foreach (var token in tokens)
        {
            string? currentValue = token[key]?.ToString();
            if (!string.IsNullOrEmpty(currentValue))
                token[key] = newLink;
        }
    }
    
    static void UpdateSumFieldsAdvanced(JObject obj, decimal newSum)
    {
        string newSumString = newSum.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

        // Ищем и обновляем строки с суммой
        foreach (var token in obj.SelectTokens("$..*").ToList())
        {
            if (token.Type == JTokenType.String)
            {
                string value = token.ToString();

                // Если строка содержит сумму в формате "x {сумма}"
                if (value.Contains("x"))
                {
                    var parts = value.Split('x');
                    if (parts.Length == 2 && decimal.TryParse(parts[1].Trim(), out _))
                    {
                        token.Replace($"{parts[0]}x {newSumString}");
                    }
                }
                // Если строка просто число
                else if (decimal.TryParse(value, out var parsedSum) && parsedSum == Math.Round(parsedSum, 2))
                {
                    token.Replace(newSumString);
                }
            }
            else if (token.Type == JTokenType.Property && ((JProperty)token).Value.Type == JTokenType.String)
            {
                var property = (JProperty)token;
                string value = property.Value.ToString();

                // Аналогичная проверка для свойства
                if (value.Contains("x"))
                {
                    var parts = value.Split('x');
                    if (parts.Length == 2 && decimal.TryParse(parts[1].Trim(), out _))
                    {
                        property.Value = $"{parts[0]}x {newSumString}";
                    }
                }
                else if (decimal.TryParse(value, out var parsedSum) && parsedSum == Math.Round(parsedSum, 2))
                {
                    property.Value = newSumString;
                }
            }
        }
    }



    // private static void UpdateQrCodeField(JObject obj, string key, string newFiscalNumber, string newF, string newS, string newT)
    // {
    //     var tokens = obj.SelectTokens($"$.rows[?(@.{key} != null)]").ToList();
    //
    //     foreach (var token in tokens)
    //     {
    //         string? currentValue = token[key]?.ToString();
    //         if (!string.IsNullOrEmpty(currentValue))
    //         {
    //             var uri = new Uri(currentValue);
    //             var query = HttpUtility.ParseQueryString(uri.Query);
    //
    //             // Обновляем параметры в строке запроса
    //             query["i"] = newFiscalNumber;
    //             query["f"] = newF;
    //             query["s"] = newS;
    //             query["t"] = newT;
    //
    //             // Формируем новый URI с обновленными параметрами
    //             var updatedUri = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}?{query}";
    //             token[key] = updatedUri;
    //         }
    //     }
    // }

}
