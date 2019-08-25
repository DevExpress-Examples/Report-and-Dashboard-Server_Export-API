namespace ExportApiDemo.Models {
    public class ExportRequestModel {
        public int Id { get; set; }
        public ExportOptions ExportOptions { get; set; }
        public DocumentParameter[] DocumentParameters { get; set; }
    }
}