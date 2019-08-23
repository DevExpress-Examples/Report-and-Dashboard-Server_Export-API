using System.Threading.Tasks;
using ExportApiDemo.Models;

namespace ExportApiDemo
{
    public interface IExportService
    {
        Task<ExportedDocumentContent> ExportReport();
        Task<ExportedDocumentContent> ExportDashboard();
        Task<ExportedDocumentContent> GetScheduledJobResult();
    }
}
