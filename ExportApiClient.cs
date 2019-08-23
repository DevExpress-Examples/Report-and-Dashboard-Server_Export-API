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
            // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            // httpClient. Add("Content-type", "application/json");
            this.httpClient = httpClient;
        }

  

        async Task<ExportedDocumentContent> IExportApiClient.ExportReport()
        {
            throw new NotImplementedException();
        }

        async Task<ExportedDocumentContent> IExportApiClient.ExportDashboard()
        {
            throw new NotImplementedException();
        }

        async Task<ExportedDocumentContent> IExportApiClient.GetJobResult()
        {
            await GetAuthToken(httpClient); // TODO: rename

            var jsonString = JsonConvert.SerializeObject(GetPdfExportModel(DemoJobResultId));
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            // Request.Headers.Add("Content-Type", "application/json");
            HttpResponseMessage downloadResponse = await httpClient.PostAsync(ServerAddress + "api/jobs/results", content);
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            // Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition { FileName = "jobResult.pdf", Inline = false }.ToString();
            return new ExportedDocumentContent(stream, "application/pdf", "jobResult.pdf");

            // return File(stream, "application/pdf");
        }

        static ExportModel GetPdfExportModel(int entityId)
        {
            return new ExportModel { Id = entityId, ExportOptions = new ExportOptions() { ExportFormat = "pdf" } };
        }

        static async Task<string> GetAuthToken(HttpClient httpClient)
        {
            string userName = "admin";
            string password = "admin";
            var body = $"grant_type=password&username={userName}&password={password}";
            var tokenResponse = await httpClient.PostAsync(ServerAddress + "oauth/token", new StringContent(body));
            tokenResponse.EnsureSuccessStatusCode();
            var authData = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<AuthData>(authData);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            return token.access_token;
        }
    }
}
