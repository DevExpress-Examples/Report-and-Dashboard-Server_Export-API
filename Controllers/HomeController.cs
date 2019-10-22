using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ExportApiDemo.Models;
using System;

namespace ExportApiDemo.Controllers {
    public class HomeController : Controller {
        readonly IExportService demoExportService;

        public HomeController(IExportService exportService) {
            this.demoExportService = exportService;
        }

        public async Task<IActionResult> ExportReport() {
            ExportedDocumentContent content = null;
            try {
                content = await demoExportService.ExportReport();
            } catch(InvalidOperationException e) {
                return BadRequest(e.Message);
            }
            return File(content.Content, content.ContentType, content.FileName);
        }

        public async Task<FileStreamResult> ExportDashboard() {
            ExportedDocumentContent content = await demoExportService.ExportDashboard();
            return File(content.Content, content.ContentType, content.FileName);
        }

        public async Task<FileStreamResult> GetScheduledJobResult() {
            ExportedDocumentContent content = await demoExportService.GetScheduledJobResult();
            return File(content.Content, content.ContentType, content.FileName);
        }

        public IActionResult Index() {
            return View();
        }
    }
}