using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Common;

namespace Core
{
    public interface IPriceProvider
    {
        Task<CurrencyCryptoRatio> GetLatestAsync(HttpClient httpClient);
    }

    public class CoinDeskPriceProvider : IPriceProvider
    {
        private readonly IJsonSerializer _json;

        public CoinDeskPriceProvider(IJsonSerializer json)
        {
            _json = json;
        }

        public async Task<CurrencyCryptoRatio> GetLatestAsync(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://api.coindesk.com");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            const string path = "/v1/bpi/currentprice.json";
            var response = await httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return _json.Deserialize<CurrencyCryptoRatio>(responseBody);
            }

            throw new Exception("currency ratio not available at the moment");
        }
    }

    public class Time
    {
        public string Updated { get; set; }
        public DateTime UpdatedIso { get; set; }
        public string Updateduk { get; set; }
    }

    public class Currency
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string Rate { get; set; }
        public string Description { get; set; }
        public string Rate_Float { get; set; }
    }

    public class CurrencyCryptoRatio
    {
        public Time Time { get; set; }
        public string Disclaimer { get; set; }
        public string ChartName { get; set; }
        public Dictionary<string, Currency> Bpi { get; set; }
    }
}