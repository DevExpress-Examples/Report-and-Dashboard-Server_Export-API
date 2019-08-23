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

namespace exportApi.Controllers
{
    public class HomeController : Controller
    {
        //NOTE: https://danieldonbavand.com/httpclientfactory-net-core-2-1/  -> more detailed description https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore
        private readonly IExportApiClient exportServiceClient;

        public HomeController(IExportApiClient exportServiceClient)
        {
            this.exportServiceClient = exportServiceClient;
        }

        const string ServerAddress = "http://localhost:8192/";
        const string ExportDocumentStatus = "api/documents/8e8b59de58874221817479a7b1cc8080/export";
        const string DownloadDocumentPath = ServerAddress + "api/documents/8e8b59de58874221817479a7b1cc8080/export/download";


        public async Task<TaskStatus> GetExportStatus()
        {
            var httpClient = new HttpClient();
            string token = await GetAuthToken(httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string statusPath = $"{ServerAddress}{ExportDocumentStatus}";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            HttpResponseMessage exportStatusResponse = await httpClient.GetAsync(statusPath);
            string statusAsString = await exportStatusResponse.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<TaskStatus>(statusAsString);

            return status;
        }

        public async Task<FileStreamResult> DownloadReport()
        {
            // var sd = await exportServiceClient.GetRandomActivity();
            //var httpClient = _httpClientFactory.CreateClient();
            var httpClient = new HttpClient();
            string token = await GetAuthToken(httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string statusPath = $"{ServerAddress}{ExportDocumentStatus}";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            HttpResponseMessage exportStatusResponse = await httpClient.GetAsync(statusPath);
            string statusAsString = await exportStatusResponse.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<TaskStatus>(statusAsString);



            HttpResponseMessage downloadResponse = await httpClient.GetAsync(DownloadDocumentPath);
            downloadResponse.EnsureSuccessStatusCode();

            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition { FileName = "report.pdf", Inline = false }.ToString();
            return File(stream, "application/pdf");
        }


        public async Task<FileStreamResult> DownloadDashboard()
        {
            var httpClient = new HttpClient();
            string token = await GetAuthToken(httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ExportModel exportModel = new ExportModel { Id = 9, ExportOptions = new ExportOptions() { ExportFormat = "pdf" } };
            var jsonString = JsonConvert.SerializeObject(exportModel);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            Request.Headers.Add("Content-Type", "application/json");

            var downloadDashboardPath = ServerAddress + "api/dashboards/export";
            HttpResponseMessage downloadResponse = await httpClient.PostAsync(downloadDashboardPath, content);
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();

            
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition { FileName = "dashboard.pdf", Inline = false }.ToString();
            return File(stream, "image/png");
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
            return token.access_token;
        }

        public async Task<FileStreamResult> GetScheduledJobResult()
        {
            var httpClient = new HttpClient();
            string token = await GetAuthToken(httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ExportModel exportModel = new ExportModel { Id = 32, ExportOptions = new ExportOptions() { ExportFormat = "pdf" } };
            var jsonString = JsonConvert.SerializeObject(exportModel);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            Request.Headers.Add("Content-Type", "application/json");
            var jobResultPath = ServerAddress + "api/jobs/results";
            HttpResponseMessage downloadResponse = await httpClient.PostAsync(jobResultPath, content);
            downloadResponse.EnsureSuccessStatusCode();
            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();
           
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition { FileName = "jobResult.pdf", Inline = false }.ToString();
            return File(stream, "application/pdf");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
