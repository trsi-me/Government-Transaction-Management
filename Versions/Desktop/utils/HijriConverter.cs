using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace STOT.Utils
{
    public static class HijriConverter
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static string GregorianToHijri(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return "";

            try
            {
                var dateObj = DateTime.Parse(dateStr);
                var year = dateObj.Year;
                var month = dateObj.Month;
                var day = dateObj.Day;

                try
                {
                    var apiUrl = $"https://api.aladhan.com/v1/gToH/{day}-{month}-{year}";
                    var response = httpClient.GetStringAsync(apiUrl).GetAwaiter().GetResult();
                    var data = JsonSerializer.Deserialize<JsonElement>(response);

                    if (data.TryGetProperty("data", out var dataObj) && dataObj.TryGetProperty("hijri", out var hijri))
                    {
                        var hYear = hijri.GetProperty("year").GetInt32();
                        var hMonth = hijri.GetProperty("month").GetProperty("number").GetInt32();
                        var hDay = hijri.GetProperty("day").GetInt32();
                        return $"{hYear}-{hMonth:D2}-{hDay:D2}";
                    }
                }
                catch { }

                return GregorianToHijriFallback(year, month, day);
            }
            catch
            {
                return "";
            }
        }

        private static string GregorianToHijriFallback(int year, int month, int day)
        {
            var hijriYear = year - 579;
            var d1 = new DateTime(year, month, day);
            var d2 = new DateTime(year, 1, 1);
            var dayOfYear = (d1 - d2).Days;
            var hijriDayOfYear = dayOfYear - (int)((year - 579) * 11.0 / 365);

            if (hijriDayOfYear < 0)
            {
                hijriYear--;
                hijriDayOfYear += 354;
            }

            var hijriMonth = (hijriDayOfYear / 29) + 1;
            if (hijriMonth > 12)
                hijriMonth = 12;

            var hijriDay = (hijriDayOfYear % 29) + 1;
            if (hijriDay > 30)
                hijriDay = 30;

            return $"{hijriYear:D4}-{hijriMonth:D2}-{hijriDay:D2}";
        }
    }
}

