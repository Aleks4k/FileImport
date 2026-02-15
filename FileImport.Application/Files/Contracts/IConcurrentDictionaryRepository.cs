using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Contracts
{
    public interface IConcurrentDictionaryRepository
    {
        Task Add(string key);
        Task Remove(string key);
        Task<List<string>> GetSubKeys(string key);
    }
}
