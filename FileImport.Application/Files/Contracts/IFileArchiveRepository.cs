using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Contracts
{
    public interface IFileArchiveRepository
    {
        Task<bool> checkFileNameFormat(string fileName);
        Task saveCheckedFile(string DocumentNumber, int user_id, string newFilePath);
    }
}
