using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.DTO
{
    public class RouteDetailsDto
    {
        public string path { get; set; } = string.Empty;
        public List<FileDetailsDto> files { get; set; } = new List<FileDetailsDto>();
    }
}
