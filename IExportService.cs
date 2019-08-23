using System.Threading.Tasks;
using exportApi.Models;

namespace exportApi
{
    public interface IExportService
    {
        Task<ExportedDocumentContent> ExportReport();
        Task<ExportedDocumentContent> ExportDashboard();
        Task<ExportedDocumentContent> GetScheduledJobResult();
    }
}
