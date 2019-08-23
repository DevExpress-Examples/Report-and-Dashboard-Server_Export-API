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

namespace ExportApiDemo {
    public class DemoExportService : IExportService {
        readonly HttpClient httpClient;

        const string ServerAddress = "http://localhost:8192/";
        const int DemoReportId = 8;
        const int DemoDashboardId = 9;
        const int DemoJobResultId = 32;
        string ExportDocumentStatusPath(string exportId) => $"api/documents/{exportId}/export";
        string DownloadDocumentPath(string exportId) => $"api/documents/{exportId}/export/download";

        public DemoExportService(HttpClient httpClient) {
            httpClient.BaseAddress = new Uri(ServerAddress);
            this.httpClient = httpClient;
        }

        public async Task<ExportedDocumentContent> ExportReport() {
            await Authorize(httpClient);

            var jsonString = JsonConvert.SerializeObject(GetPdfExportModel(DemoReportId));
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage startExportResponse = await httpClient.PostAsync(ServerAddress + "api/reports/export", content);
            startExportResponse.EnsureSuccessStatusCode();
            string exportIdJson = await startExportResponse.Content.ReadAsStringAsync();
            var exportReportModel = JsonConvert.DeserializeObject<ReportExportInfo>(exportIdJson);

            string statusPath = $"{ServerAddress}{ExportDocumentStatusPath(exportReportModel.ExportId)}";
            TaskStatus status = TaskStatus.InProgress;

            while(status != TaskStatus.Complete) {
                Thread.Sleep(500);
                HttpResponseMessage exportStatusResponse = await httpClient.GetAsync(statusPath);
                status = JsonConvert.DeserializeObject<TaskStatus>(await exportStatusResponse.Content.ReadAsStringAsync());
            }

            HttpResponseMessage downloadResponse = await httpClient.GetAsync(ServerAddress + DownloadDocumentPath(exportReportModel.ExportId));
            downloadResponse.EnsureSuccessStatusCode();

            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "report.pdf");
        }

        public async Task<ExportedDocumentContent> ExportDashboard() {
            await Authorize(httpClient);

            var jsonString = JsonConvert.SerializeObject(GetPdfExportModel(DemoDashboardId));
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage downloadResponse = await httpClient.PostAsync(ServerAddress + "api/dashboards/export", content);
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "dashboard.pdf");
        }

        public async Task<ExportedDocumentContent> GetScheduledJobResult() {
            await Authorize(httpClient);

            var jsonString = JsonConvert.SerializeObject(GetPdfExportModel(DemoJobResultId));
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage downloadResponse = await httpClient.PostAsync(ServerAddress + "api/jobs/results", content);
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "jobResult.pdf");
        }

        static ExportRequestModel GetPdfExportModel(int entityId) {
            return new ExportRequestModel { Id = entityId, ExportOptions = new ExportOptions() { ExportFormat = "pdf" } };
        }

        static async Task Authorize(HttpClient httpClient) {
            string demoAccountUserName = "admin";
            string demoAccountPassword = "admin";
            var authRequestBody = $"grant_type=password&username={demoAccountUserName}&password={demoAccountPassword}";
            var authResponse = await httpClient.PostAsync(ServerAddress + "oauth/token", new StringContent(authRequestBody));
            authResponse.EnsureSuccessStatusCode();
            var token = JsonConvert.DeserializeObject<Token>(await authResponse.Content.ReadAsStringAsync()).AuthToken;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
