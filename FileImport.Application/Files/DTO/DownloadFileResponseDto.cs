using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.DTO
{
    public class DownloadFileResponseDto
    {
        public Stream FileStream { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
