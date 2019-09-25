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

        const string ServerAddress = "https://reportserver.devexpress.com/";
        const string DemoAccountUserName = "Guest";
        const string DemoAccountPassword = "";
        const int DemoReportId = 8;
        const int DemoDashboardId = 9;
        const int DemoJobResultId = 32;
        string DocumentExportStatusPath(string exportId) => $"api/documents/export/status/{exportId}";
        string DownloadDocumentPath(string exportId) => $"api/documents/export/result/{exportId}";

        public DemoExportService(HttpClient httpClient) {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri(ServerAddress);
        }

        public async Task<ExportedDocumentContent> ExportReport() {
            await Authorize(httpClient);

            // Start report export task
            var documentParameters = new DocumentParameter[] { new DocumentParameter() { Name = "CustomerID", Value = "CACTU" } };
            HttpContent startExportResponse = await GetExportResponseContent($"api/documents/export/report/{DemoReportId}", GetPdfExportModel(documentParameters));

            // Check report export task status
            var reportExportInfo = JsonConvert.DeserializeObject<ReportExportInfo>(await startExportResponse.ReadAsStringAsync());
            TaskStatus status = TaskStatus.InProgress;
            while(status != TaskStatus.Complete) {
                Thread.Sleep(500);
                HttpResponseMessage exportStatusResponse = await httpClient.GetAsync($"{DocumentExportStatusPath(reportExportInfo.ExportId)}");
                status = JsonConvert.DeserializeObject<TaskStatus>(await exportStatusResponse.Content.ReadAsStringAsync());
            }

            // Download exported report 
            HttpResponseMessage downloadResponse = await httpClient.GetAsync(DownloadDocumentPath(reportExportInfo.ExportId));
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "report.pdf");
        }

        public async Task<ExportedDocumentContent> ExportDashboard() {
            await Authorize(httpClient);
            return await GetExportedDocumentContent(GetPdfExportModel(), $"api/documents/export/dashboard/{DemoDashboardId}", "dashboard.pdf");
        }

        public async Task<ExportedDocumentContent> GetScheduledJobResult() {
            await Authorize(httpClient);
            return await GetExportedDocumentContent(new ExportOptions() { ExportFormat = "pdf" }, $"api/jobs/results/{DemoJobResultId}", "jobResult.pdf");
        }

        async Task<ExportedDocumentContent> GetExportedDocumentContent(object exportRequestModel, string exportUrl, string fileName) {
            var content = await GetExportResponseContent(exportUrl, exportRequestModel);
            return new ExportedDocumentContent(await content.ReadAsStreamAsync(), "application/pdf", fileName);
        }

        async Task<HttpContent> GetExportResponseContent(string exportUrl, object exportRequestModel) {
            var content = new StringContent(JsonConvert.SerializeObject(exportRequestModel), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(exportUrl, content);
            response.EnsureSuccessStatusCode();
            return response.Content;
        }

        static ExportRequestModel GetPdfExportModel(DocumentParameter[] parameters = null) {
            return new ExportRequestModel {
                ExportOptions = new ExportOptions() { ExportFormat = "pdf" },
                DocumentParameters = parameters
            };
        }

        static async Task Authorize(HttpClient httpClient) {
            var authRequestBody = $"grant_type=password&username={DemoAccountUserName}&password={DemoAccountPassword}";
            var authResponse = await httpClient.PostAsync("oauth/token", new StringContent(authRequestBody));
            authResponse.EnsureSuccessStatusCode();
            var token = JsonConvert.DeserializeObject<Token>(await authResponse.Content.ReadAsStringAsync()).AuthToken;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
