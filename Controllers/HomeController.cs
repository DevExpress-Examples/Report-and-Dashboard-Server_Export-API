using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using exportApi.Models;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;

namespace exportApi.Controllers
{
    public class HomeController : Controller
    {
        readonly IExportService demoExportService;

        public HomeController(IExportService exportService)
        {
            this.demoExportService = exportService;
        }

        public async Task<FileStreamResult> ExportReport()
        {
            ExportedDocumentContent content = await demoExportService.ExportReport();
            return File(content.Content, content.ContentType, content.FileName);
        }

        public async Task<FileStreamResult> ExportDashboard()
        {
            ExportedDocumentContent content = await demoExportService.ExportDashboard();
            return File(content.Content, content.ContentType, content.FileName);
        }

        public async Task<FileStreamResult> GetScheduledJobResult()
        {
            ExportedDocumentContent content = await demoExportService.GetJobResult();
            return File(content.Content, content.ContentType, content.FileName);
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