using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Infrastructure.Settings
{
    public class FileStorageOptions
    {
        public string RootPath { get; set; } = string.Empty;
        public string RootPathChecked { get; set; } = string.Empty;
    }
}
