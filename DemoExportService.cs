using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using ExportApiDemo.Models;
using System.Text;
using System.Net.Http.Headers;
using TaskStatus = ExportApiDemo.Models.TaskStatus;
using System.Threading;

namespace ExportApiDemo
{
    public class DemoExportService : IExportService
    {
        readonly HttpClient httpClient;

        const string ServerAddress = "http://localhost:8192/";
        const int DemoReportId = 8;
        const int DemoDashboardId = 9;
        const int DemoJobResultId = 32;
        string ExportDocumentStatusPath(string exportId) => $"api/documents/{exportId}/export";
        string DownloadDocumentPath(string exportId) => $"api/documents/{exportId}/export/download";

        public DemoExportService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri(ServerAddress);
        }

        public async Task<ExportedDocumentContent> ExportReport()
        {
            await Authorize(httpClient);

            HttpContent content = await GetRequestContent(DemoReportId, "api/reports/export");
            var exportReportModel = JsonConvert.DeserializeObject<ReportExportInfo>(await content.ReadAsStringAsync());
            TaskStatus status = TaskStatus.InProgress;
            
            while (status != TaskStatus.Complete)
            {
                Thread.Sleep(500);
                HttpResponseMessage exportStatusResponse = await httpClient.GetAsync($"{ExportDocumentStatusPath(exportReportModel.ExportId)}");
                status = JsonConvert.DeserializeObject<TaskStatus>(await exportStatusResponse.Content.ReadAsStringAsync());
            }

            HttpResponseMessage downloadResponse = await httpClient.GetAsync(DownloadDocumentPath(exportReportModel.ExportId));
            downloadResponse.EnsureSuccessStatusCode();

            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "report.pdf");
        }

      

        public async Task<ExportedDocumentContent> ExportDashboard()
        {
            await Authorize(httpClient);

            return await GetExportedDocumentContent(DemoDashboardId,"api/dashboards/export", "dashboard.pdf");
        }
        

        public async Task<ExportedDocumentContent> GetScheduledJobResult()
        {
            await Authorize(httpClient);

            return await GetExportedDocumentContent(DemoJobResultId, "api/jobs/results", "jobResult.pdf");
        }



        async Task<ExportedDocumentContent> GetExportedDocumentContent(int entityId, string exportUrl, string fileName)
        {
            // var content = new StringContent(JsonConvert.SerializeObject(GetPdfExportModel(entityId)), Encoding.UTF8, "application/json");
            // HttpResponseMessage response = await httpClient.PostAsync(exportUrl, content);
            // response.EnsureSuccessStatusCode();

            var content = await GetRequestContent(entityId, exportUrl);
            return new ExportedDocumentContent(await content.ReadAsStreamAsync(), "application/pdf", fileName);
        }

        async Task<HttpContent> GetRequestContent(int entityId, string exportUrl)
        {
            var content = new StringContent(JsonConvert.SerializeObject(GetPdfExportModel(entityId)), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(exportUrl, content);
            response.EnsureSuccessStatusCode();
            return response.Content;
        }

        static ExportRequestModel GetPdfExportModel(int entityId)
        {
            return new ExportRequestModel { Id = entityId, ExportOptions = new ExportOptions() { ExportFormat = "pdf" } };
        }

        static async Task Authorize(HttpClient httpClient)
        {
            string demoAccountUserName = "admin";
            string demoAccountPassword = "admin";
            var authRequestBody = $"grant_type=password&username={demoAccountUserName}&password={demoAccountPassword}";
            var authResponse = await httpClient.PostAsync("oauth/token", new StringContent(authRequestBody));
            authResponse.EnsureSuccessStatusCode();
            var token = JsonConvert.DeserializeObject<Token>(await authResponse.Content.ReadAsStringAsync()).AuthToken;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
