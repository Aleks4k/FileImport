using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.DTO
{
    public class FileDetailsDto
    {
        public string name { get; set; } = string.Empty; //123.pdf
        public bool isFolder { get; set; } = false; //Samo ako je folder.
    }
}
