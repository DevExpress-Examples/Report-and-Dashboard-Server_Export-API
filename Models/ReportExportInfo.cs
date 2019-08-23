using Newtonsoft.Json;

namespace ExportApiDemo.Models {
    public class ReportExportInfo {
        [JsonProperty("exportId")]
        public string ExportId { get; set; }
    }
}