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
using exportApi.Models;
using System.Text;
using System.Net.Http.Headers;

namespace exportApi
{
    public interface IExportApiClient
    {
        Task<ExportedDocumentContent> ExportReport();
        Task<ExportedDocumentContent> ExportDashboard();
        Task<ExportedDocumentContent> GetJobResult();
    }


    //NOTE: https://danieldonbavand.com/httpclientfactory-net-core-2-1/  -> more detailed description https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore
    public class ExportApiClient : IExportApiClient
    {
        readonly HttpClient httpClient;

        const string ServerAddress = "http://localhost:8192/";
        const int DemoReportId = 8;
        const int DemoDashboardId = 9;
        const int DemoJobResultId = 32;

        // const string DemoReportExportId = "95f71138eb424dc0a8c17da141496c93";
        // const string DownloadDocumentPath = ServerAddress + "api/documents/95f71138eb424dc0a8c17da141496c93/export/download";
        string ExportDocumentStatus(string exportId) => $"api/documents/{exportId}/export";
        string DownloadDocumentPath(string exportId) => $"api/documents/{exportId}/export/download";

        public ExportApiClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(ServerAddress);
            this.httpClient = httpClient;
        }

        async Task<ExportedDocumentContent> IExportApiClient.ExportReport()
        {
            throw new NotImplementedException();
        }

        async Task<ExportedDocumentContent> IExportApiClient.ExportDashboard()
        {
            await Authorize(httpClient);
            var jsonString = JsonConvert.SerializeObject(GetPdfExportModel(DemoDashboardId));
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage downloadResponse = await httpClient.PostAsync(ServerAddress + "api/dashboards/export", content);
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "dashboard.pdf");
        }

        async Task<ExportedDocumentContent> IExportApiClient.GetJobResult()
        {
            await Authorize(httpClient);

            var jsonString = JsonConvert.SerializeObject(GetPdfExportModel(DemoJobResultId));
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage downloadResponse = await httpClient.PostAsync(ServerAddress + "api/jobs/results", content);
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "jobResult.pdf");
        }

        static ExportModel GetPdfExportModel(int entityId)
        {
            return new ExportModel { Id = entityId, ExportOptions = new ExportOptions() { ExportFormat = "pdf" } };
        }

        static async Task<string> Authorize(HttpClient httpClient)
        {
            string demoAccountUserName = "admin";
            string demoAccountPassword = "admin";
            var authRequestBodu = $"grant_type=password&username={demoAccountUserName}&password={demoAccountPassword}";
            var authResponse = await httpClient.PostAsync(ServerAddress + "oauth/token", new StringContent(authRequestBodu));
            authResponse.EnsureSuccessStatusCode();
            var token = JsonConvert.DeserializeObject<AuthData>(await authResponse.Content.ReadAsStringAsync()).access_token;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return token;
        }
    }
}
