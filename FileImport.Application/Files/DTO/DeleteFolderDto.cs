using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.DTO
{
    public class DeleteFolderDto
    {
        public required string path { get; set; } = string.Empty;
        public required bool notEmptyAgree { get; set; } = false;
    }
}
