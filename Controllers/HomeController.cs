using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using exportApi.Models;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Threading;
using TaskStatus = exportApi.Models.TaskStatus;

namespace exportApi.Controllers
{
    public class HomeController : Controller
    {
        readonly IExportApiClient exportServiceClient;

        public HomeController(IExportApiClient exportServiceClient)
        {
            this.exportServiceClient = exportServiceClient;
        }

        const string ServerAddress = "http://localhost:8192/";
        const int DemoReportId = 8;
        const int DemoDashboardId = 9;
        const int DemoJobResultId = 32;

        // const string DemoReportExportId = "95f71138eb424dc0a8c17da141496c93";
        // const string DownloadDocumentPath = ServerAddress + "api/documents/95f71138eb424dc0a8c17da141496c93/export/download";
        string ExportDocumentStatus(string exportId) => $"api/documents/{exportId}/export";
        string DownloadDocumentPath(string exportId) => $"api/documents/{exportId}/export/download";


        static async Task<string> GetAuthToken(HttpClient httpClient)
        {
            string userName = "admin";
            string password = "admin";
            var body = $"grant_type=password&username={userName}&password={password}";
            var tokenResponse = await httpClient.PostAsync(ServerAddress + "oauth/token", new StringContent(body));
            tokenResponse.EnsureSuccessStatusCode();
            var authData = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<AuthData>(authData);
            return token.access_token;
        }

        public async Task<FileStreamResult> ExportReport()
        {
            //exportServiceClient.ExportReport();

            // var sd = await exportServiceClient.GetRandomActivity();
            //var httpClient = _httpClientFactory.CreateClient();
            var httpClient = new HttpClient();
            string token = await GetAuthToken(httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ExportModel exportModel = new ExportModel { Id = DemoReportId, ExportOptions = new ExportOptions() { ExportFormat = "pdf" } };
            var jsonString = JsonConvert.SerializeObject(exportModel);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            Request.Headers.Add("Content-Type", "application/json");

            var startExportPath = ServerAddress + "api/reports/export";
            HttpResponseMessage startExportResponse = await httpClient.PostAsync(startExportPath, content);
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
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition { FileName = "report.pdf", Inline = false }.ToString();
            return File(stream, "application/pdf");
        }

        public async Task<FileStreamResult> ExportDashboard()
        {
            Request.Headers.Add("Content-Type", "application/json");
            ExportedDocumentContent content = await exportServiceClient.ExportDashboard();
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition { FileName = content.FileName, Inline = false }.ToString();
            return File(content.Content, content.ContentType);
        }

        public async Task<FileStreamResult> GetScheduledJobResult()
        {
            Request.Headers.Add("Content-Type", "application/json");
            ExportedDocumentContent content = await exportServiceClient.GetJobResult();
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition { FileName = content.FileName, Inline = false }.ToString();
            return File(content.Content, content.ContentType);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}






// public async Task<TaskStatus> GetExportStatus(string exportId)
// {
//     var httpClient = new HttpClient();
//     string token = await GetAuthToken(httpClient);
//     httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

//     string statusPath = $"{ServerAddress}{ExportDocumentStatus(exportId)}";
//     var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
//     HttpResponseMessage exportStatusResponse = await httpClient.GetAsync(statusPath);
//     string statusAsString = await exportStatusResponse.Content.ReadAsStringAsync();
//     var status = JsonConvert.DeserializeObject<TaskStatus>(statusAsString);

//     return status;
// }