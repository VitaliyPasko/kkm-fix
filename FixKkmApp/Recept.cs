using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FixKkmApp;

public class Recept
{
    public string Update(string json, string? newFiscalNumber = null, DateTime? newCreationDate = null,
        string? newbase64 = null, string? newLink = null, decimal? newPrice = null)
    {
        // Парсим JSON в объект
        JObject obj = JObject.Parse(json);

        if (!string.IsNullOrWhiteSpace(newFiscalNumber))
        {
            // 1. Изменение поля "center": "<#FiscalNumber#>/<%FiscalNumber%>:"
            UpdateChastichnoField(obj, "center", newFiscalNumber, "<#FiscalNumber#>/<%FiscalNumber%>:");

            // 1. Изменение поля "center": "<#OfflineFiscalNumber#>/<%OfflineFiscalNumber%>:"
            UpdateChastichnoField(obj, "center", newFiscalNumber, "<#OfflineFiscalNumber#>/<%OfflineFiscalNumber%>:");
        }

        if (newCreationDate.HasValue)
        {
            // 2. Изменение поля "center": "<#Time#>/<%Time%>:" 11/30/2024 23:28:32
            UpdateChastichnoField(obj, key: "center",
                newCreationDate.Value.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture)
                ,
                matchValue: "<#Time#>/<%Time%>:");

            // "dateCreation": "2024-11-30T23:28:32.153674Z"
            UpdateCreationDateTimeField(obj, key: "dateCreation",
                newValue: newCreationDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"));
        }

        if (!string.IsNullOrWhiteSpace(newbase64))
        {
            // 3. Изменение base64 в поле "center"
            UpdateBase64Field(obj, "center", newbase64);
        }

        if (!string.IsNullOrWhiteSpace(newLink))
        {
            // Замена динамических параметров в поле "qrcode"
            // UpdateQrCodeField(obj, "qrcode", newLink);
            UpdateField(obj, key: "qrcode", newValue: newLink);
        }

        if (newPrice.HasValue)
            UpdateSumFieldsAdvanced(obj, newPrice.Value);

        // Вывод измененного JSON
        // Приведение JObject к object для корректного вызова SerializeObject
        string updatedJson = JsonConvert.SerializeObject(obj, Formatting.Indented);
        Console.WriteLine(
            "-------------------------------------------------------------------------------------------------------------");
        Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.None));
        Console.WriteLine(
            "-------------------------------------------------------------------------------------------------------------");
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

    static void UpdateCreationDateTimeField(JObject obj, string key, string newValue)
    {
        // Замена значения ключа dateCreation
        obj[key] = newValue;
    }

    static void UpdateChastichnoField(JObject obj, string key, string newValue, string matchValue)
    {
        var tokens = obj.SelectTokens($"$.rows[?(@.{key} != null)]").ToList();
        foreach (var token in tokens.Where(token => token[key]?.ToString().Contains(matchValue) == true))
        {
            token[key] = $"{matchValue} {newValue}";
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

    static void UpdateSumFieldsAdvanced(JObject obj, decimal newSum)
    {
        string newSumString = newSum.ToString("0.00", CultureInfo.InvariantCulture);

        // Рекурсивный обход всех узлов JSON
        foreach (var token in obj.Descendants().ToList())
        {
            if (token.Type == JTokenType.String) // Проверяем только строковые значения
            {
                string value = token.ToString();

                // Проверяем, содержит ли строка формат "x {сумма}"
                if (value.Contains('x'))
                {
                    var parts = value.Split('x');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[1].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture,
                            out var parsedSum) &&
                        parts[1].Trim() == parsedSum.ToString("0.00", CultureInfo.InvariantCulture))
                    {
                        token.Replace($"{parts[0].Trim()} x {newSumString}");
                    }
                }
                // Если строка просто является числом (суммой) и имеет формат "0.00"
                else if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture,
                             out var parsedValue) &&
                         value == parsedValue.ToString("0.00", CultureInfo.InvariantCulture))
                {
                    token.Replace(newSumString);
                }
            }
        }
    }
}