using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.DTO
{
    public class RenameFolderDto
    {
        public required string path { get; set; } = string.Empty;
        public required string newName { get; set; } = string.Empty;
    }
}
