using System;
using System.IO;

namespace exportApi.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class ExportedDocumentContent
    {
        readonly Stream content;
        readonly string contentType;
        readonly string fileName;

        public ExportedDocumentContent(Stream content, string contentType, string fileName)
        {
            this.content = content;
            this.contentType = contentType;
            this.fileName = fileName;
        }

        public Stream Content { get { return content; } }
        public string ContentType { get { return contentType; } }
        public string FileName { get { return fileName; } }
    }

    public class StartExportModel
    {
        public string exportId { get; set; }
    }

    public class ExportModel
    {
        public int Id { get; set; }
        public ExportOptions ExportOptions { get; set; }
    }

    public enum TaskStatus
    {
        Fault = 0,
        InProgress = 1,
        Complete = 2
    }

    public class ExportOptions
    {
        public string ExportFormat { get; set; }
    }

    public class AuthData
    {
        public string access_token { get; set; }
    }
}