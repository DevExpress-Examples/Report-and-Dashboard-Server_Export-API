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


namespace exportApi.Controllers
{

    public enum TaskStatus
    {
        Fault = 0,
        InProgress = 1,
        Complete = 2
    }

    public class HomeController : Controller
    {
        //NOTE: https://danieldonbavand.com/httpclientfactory-net-core-2-1/ 
        private readonly IHttpClientFactory _httpClientFactory;
        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        const string ServerAddress = "http://localhost:8192/";
        const string ExportDocumentStatus = "api/documents/8e8b59de58874221817479a7b1cc8080/export";

        const string DownloadDocumentPath = ServerAddress + "api/documents/8e8b59de58874221817479a7b1cc8080/export/download";


        public async Task<FileStreamResult> DownloadReport()
        {
            var httpClient = _httpClientFactory.CreateClient();
            // var httpClient = new HttpClient();

            string statusPath = $"{ServerAddress}{ExportDocumentStatus}";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            HttpResponseMessage exportStatusResponse = await httpClient.GetAsync(statusPath);
            string statusAsString = await exportStatusResponse.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<TaskStatus>(statusAsString);

            HttpResponseMessage downloadResponse = await httpClient.GetAsync(DownloadDocumentPath);
            downloadResponse.EnsureSuccessStatusCode();

            Stream stream = await downloadResponse.Content.ReadAsStreamAsync();

            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "report.pdf",
                Inline = false
            };

            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(stream, "application/pdf");

            // .ContinueWith(x =>
            // {
            //     Console.WriteLine("file saved at " + AppDomain.CurrentDomain.BaseDirectory);
            //     var fileStream = File.Create(AppDomain.CurrentDomain.BaseDirectory + "File.pdf");
            //     x.Result.CopyToAsync(fileStream).ContinueWith(y => { fileStream.Close(); x.Result.Close(); });
            // });

            // return statusAsString;

            // HttpResponseMessage downloadResponse = await httpClient.GetAsync(reqPath);
            // downloadResponse.EnsureSuccessStatusCode();

            // await downloadResponse.Content.ReadAsStreamAsync().ContinueWith(x =>
            // {

            //     Console.WriteLine("file saved at " + AppDomain.CurrentDomain.BaseDirectory);
            //     var fileStream = System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "File.pdf");

            //     x.Result.CopyToAsync(fileStream).ContinueWith(y => { fileStream.Close(); x.Result.Close(); });

            //     return q;
            // });


            // HttpResponseMessage str = httpClient.GetAsync(reqPath).Result;

            // return str.Content;
            // return File(new MemoryStream(), "application/pdf");
        }


        public async Task<FileStreamResult> DownloadDashboard()
        {
            var httpClient = new HttpClient();

            string statusPath = $"{ServerAddress}{ExportDocumentStatus}";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            HttpResponseMessage exportStatusResponse = await httpClient.GetAsync(statusPath);
            string statusAsString = await exportStatusResponse.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<TaskStatus>(statusAsString);

            HttpResponseMessage downloadResponse = await httpClient.GetAsync(DownloadDocumentPath);
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
