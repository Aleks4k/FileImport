using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.DTO
{
    public class RequestDownloadQueryDto
    {
        public required string path { get; set; } = string.Empty;
    }
}
