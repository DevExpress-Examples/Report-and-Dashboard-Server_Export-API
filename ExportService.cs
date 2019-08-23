using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using exportApi.Models;
using System.Text;
using System.Net.Http.Headers;
using TaskStatus = exportApi.Models.TaskStatus;
using System.Threading;

namespace exportApi
{
    public interface IExportService
    {
        Task<ExportedDocumentContent> ExportReport();
        Task<ExportedDocumentContent> ExportDashboard();
        Task<ExportedDocumentContent> GetJobResult();
    }

    //NOTE: https://danieldonbavand.com/httpclientfactory-net-core-2-1/  -> more detailed description https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore
    public class ExportService : IExportService
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

        public ExportService(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(ServerAddress);
            this.httpClient = httpClient;
        }

        async Task<ExportedDocumentContent> IExportService.ExportReport()
        {
            await Authorize(httpClient);

            var jsonString = JsonConvert.SerializeObject(GetPdfExportModel(DemoReportId));
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage startExportResponse = await httpClient.PostAsync(ServerAddress + "api/reports/export", content);
            startExportResponse.EnsureSuccessStatusCode();
            string exportIdJson = await startExportResponse.Content.ReadAsStringAsync();
            var exportReportModel = JsonConvert.DeserializeObject<StartExportModel>(exportIdJson);

            string statusPath = $"{ServerAddress}{ExportDocumentStatus(exportReportModel.exportId)}";
            TaskStatus status = TaskStatus.InProgress;

            while (status != TaskStatus.Complete)
            {
                Thread.Sleep(500);
                HttpResponseMessage exportStatusResponse = await httpClient.GetAsync(statusPath);
                status = JsonConvert.DeserializeObject<TaskStatus>(await exportStatusResponse.Content.ReadAsStringAsync());
            }

            HttpResponseMessage downloadResponse = await httpClient.GetAsync(ServerAddress + DownloadDocumentPath(exportReportModel.exportId));
            downloadResponse.EnsureSuccessStatusCode();

            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "report.pdf");
        }

        async Task<ExportedDocumentContent> IExportService.ExportDashboard()
        {
            await Authorize(httpClient);

            var jsonString = JsonConvert.SerializeObject(GetPdfExportModel(DemoDashboardId));
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage downloadResponse = await httpClient.PostAsync(ServerAddress + "api/dashboards/export", content);
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            return new ExportedDocumentContent(stream, "application/pdf", "dashboard.pdf");
        }

        async Task<ExportedDocumentContent> IExportService.GetJobResult()
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

        static async Task Authorize(HttpClient httpClient)
        {
            string demoAccountUserName = "admin";
            string demoAccountPassword = "admin";
            var authRequestBodu = $"grant_type=password&username={demoAccountUserName}&password={demoAccountPassword}";
            var authResponse = await httpClient.PostAsync(ServerAddress + "oauth/token", new StringContent(authRequestBodu));
            authResponse.EnsureSuccessStatusCode();
            var token = JsonConvert.DeserializeObject<Token>(await authResponse.Content.ReadAsStringAsync()).AuthToken;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
