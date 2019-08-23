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
    public class ExportModel
    {
        public int Id { get; set; }
        public ExportOptions ExportOptions { get; set; }
    }

    public class ExportOptions
    {
        public string ExportFormat { get; set; }
    }
    public class AuthData
    {
        public string access_token { get; set; }
    }


    public enum TaskStatus
    {
        Fault = 0,
        InProgress = 1,
        Complete = 2
    }

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


        public async Task<FileStreamResult> DownloadReport()
        {
            // var response = await _boardApiClient.Client;

            // var sd = await exportServiceClient.GetRandomActivity();

            //var httpClient = _httpClientFactory.CreateClient();
            var httpClient = new HttpClient();

            string statusPath = $"{ServerAddress}{ExportDocumentStatus}";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            HttpResponseMessage exportStatusResponse = await httpClient.GetAsync(statusPath);
            string statusAsString = await exportStatusResponse.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<TaskStatus>(statusAsString);

            HttpResponseMessage downloadResponse = await httpClient.GetAsync(DownloadDocumentPath);
            downloadResponse.EnsureSuccessStatusCode();

            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();

            var fileName = "report.pdf";

            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition { FileName = fileName, Inline = false }.ToString();
            // System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            // {
            //     FileName = "report.pdf",
            //     Inline = false
            // };

            // Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(stream, "application/pdf");
        }


        public async Task<FileStreamResult> DownloadDashboard()
        {
            var httpClient = new HttpClient();


            string userName = "admin";
            string password = "admin";
            var body = $"grant_type=password&username={userName}&password={password}";

            var tokenResponse = await httpClient.PostAsync(ServerAddress + "oauth/token", new StringContent(body));
            tokenResponse.EnsureSuccessStatusCode();
            var authData = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<AuthData>(authData);

            var downloadDashboardPath = ServerAddress + "api/dashboards/export";

            ExportModel exportModel = new ExportModel { Id = 9, ExportOptions = new ExportOptions() { ExportFormat = "pdf" } };
            // ExportModel exportModel = new ExportModel { Id = 9 };
            var jsonString = JsonConvert.SerializeObject(exportModel);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");


            var sd = HttpContext.Request.Headers["Authorization"];
            Request.Headers.Add("Content-Type", "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);



            // Request.Headers.Add("Authorization", $"Bearer {token.access_token}");
            var sdd = HttpContext.Request.Headers["Authorization"];
            var sdds = Request.Headers["Authorization"];
            
            HttpResponseMessage downloadResponse = await httpClient.PostAsync(downloadDashboardPath, content);
            downloadResponse.EnsureSuccessStatusCode();

            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();

            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "dashboard.pdf",
                Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };

            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(stream, "image/png");
        }



        public async Task<FileStreamResult> GetScheduledJobResult()
        {
            throw new NotImplementedException();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
