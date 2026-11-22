using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MorningWidget
{
    public record WeatherData(string Temp, string Description, string Details);
    public record CurrencyData(string Usd, string Eur, string Cny);

    public class ApiService
    {
        private const string LAT = "55.77"; // Павловский Посад
        private const string LON = "38.65";
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<WeatherData> GetWeatherAsync()
        {
            try
            {
                string host = string.Concat("https://", "api.", "open-meteo.", "com/v1/forecast");
                string url = $"{host}?latitude={LAT}&longitude={LON}&current=temperature_2m,relative_humidity_2m,weather_code,wind_speed_10m&wind_speed_unit=ms";

                using JsonDocument doc = JsonDocument.Parse(await httpClient.GetStringAsync(url));
                var current = doc.RootElement.GetProperty("current");

                double temp = current.GetProperty("temperature_2m").GetDouble();
                int humidity = current.GetProperty("relative_humidity_2m").GetInt32();
                double wind = current.GetProperty("wind_speed_10m").GetDouble();
                int code = current.GetProperty("weather_code").GetInt32();

                string tempStr = $"{(temp > 0 ? "+" : "")}{Math.Round(temp)}°";
                string details = $"Влажность: {humidity}%  •  Ветер: {wind} м/с";
                string desc = code switch
                {
                    0 => "Ясно ☀️",
                    1 or 2 or 3 => "Переменная облачность ⛅",
                    45 or 48 => "Туман 🌫️",
                    51 or 53 or 55 or 61 or 63 or 65 => "Дождь 🌧️",
                    71 or 73 or 75 or 77 or 85 or 86 => "Снег ❄️",
                    95 or 96 or 99 => "Гроза ⛈️",
                    _ => "Пасмурно ☁️"
                };

                return new WeatherData(tempStr, desc, details);
            }
            catch
            {
                return new WeatherData("--°", "Нет подключения", "Влажность: --%  •  Ветер: -- м/с");
            }
        }

        public async Task<CurrencyData> GetCurrencyRatesAsync()
        {
            try
            {
                string url = string.Concat("https://", "www.", "cbr-xml-daily.", "ru/daily_json.js");
                using JsonDocument doc = JsonDocument.Parse(await httpClient.GetStringAsync(url));
                var valute = doc.RootElement.GetProperty("Valute");

                string usd = $"{valute.GetProperty("USD").GetProperty("Value").GetDouble():F2} ₽";
                string eur = $"{valute.GetProperty("EUR").GetProperty("Value").GetDouble():F2} ₽";
                string cny = $"{valute.GetProperty("CNY").GetProperty("Value").GetDouble():F2} ₽";

                return new CurrencyData(usd, eur, cny);
            }
            catch
            {
                return new CurrencyData("Ошибка", "Ошибка", "Ошибка");
            }
        }
    }
}
