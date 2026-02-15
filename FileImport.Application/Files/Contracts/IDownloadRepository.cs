using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Contracts
{
    public interface IDownloadRepository
    {
        Task<string> getFilePathFromKey(string key);
        Task<string> GenerateGUID(string key);
        Task<bool> checkIfKeyExists(string key);
    }
}
