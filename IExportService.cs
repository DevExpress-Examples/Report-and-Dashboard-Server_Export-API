using System.Threading.Tasks;
using ExportApi.Models;

namespace ExportApi
{
    public interface IExportService
    {
        Task<ExportedDocumentContent> ExportReport();
        Task<ExportedDocumentContent> ExportDashboard();
        Task<ExportedDocumentContent> GetScheduledJobResult();
    }
}
