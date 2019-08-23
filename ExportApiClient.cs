using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

namespace exportApi
{
    public interface IExportApiClient
    {
        Task<object> GetRandomActivity();
    }

    public class ExportApiClient : IExportApiClient
    {
        const string ExportDocumentStatus = "documents/95f71138eb424dc0a8c17da141496c93/export";

        readonly HttpClient _client;

        public ExportApiClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://localhost:8192/api/");
            // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            // httpClient. Add("Content-type", "application/json");
            _client = httpClient;
        }

        public async Task<object> GetRandomActivity()
        {
            var response = await _client.GetAsync(ExportDocumentStatus);

            return JsonConvert.DeserializeObject<object>(null);
        }

        public async Task<object> GetCatalogItems(int page, int take, int? brand, int? type)
        {

            var responseString = await _client.GetStringAsync("uri");

            var catalog = JsonConvert.DeserializeObject<object>(responseString);
            return catalog;
        }
    }
}
