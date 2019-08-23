namespace ExportApiDemo.Models {
    public class ErrorViewModel {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class StartExportModel {
        public string exportId { get; set; }
    }

    public class ExportModel {
        public int Id { get; set; }

        public ExportOptions ExportOptions { get; set; }
    }
}