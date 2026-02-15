using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.DTO
{
    public class GetFolderSubFoldersDto
    {
        public required string path { get; set; } = string.Empty;
    }
}
