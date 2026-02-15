using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Contracts
{
    public interface IFileRepository
    {
        Task WriteFilesToCache(string key, List<FileDetailsDto> files);
        Task<List<FileDetailsDto>> getFilesFromKey(string key);
        Task<bool> IsKeyMapped(string key);
        Task<bool> DoesFolderHasSubFolders(string key);
        Task<List<FileDetailsDto>> GetFolderSubFolders(string key);
        Task DeleteKey(string key, bool recursive);
        Task DeleteFile(string fileName, string pathKey);
        Task RenameFile(string fileName, string pathKey, string newName);
        Task RenameKey(string key, string newKey, bool recursive);
    }
}
