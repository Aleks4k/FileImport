using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Contracts
{
    public interface IFileService
    {
        Task<RouteDetailsDto> getAllFilesFromRootPath();
        Task<RouteDetailsDto> getAllFilesFromPath(string path);
        Task<bool> checkIfDirectoryExists(string path);
        Task<bool> checkIfFileExists(string path);
        Task<bool> checkNewFile(string path, string newName);
        Task<bool> checkNewFolder(string path, string newName);
        Task<bool> checkIfDirectoryHasSubFolders(string path);
        Task<bool> checkIfDirectoryHasSubFiles(string path);
        Task<List<FileDetailsDto>> getDirectorySubFolders(string path);
        Task<DownloadFileResponseDto> getFileDownload(string path);
        Task<string> getCleanRelativePath(string path); //Argument jeste relativni path ali ne znači da nema neke simbole viška a u FileRepository se piše baš u ovom formatu koji vraća f-ja.
        Task<string> getCleanRelativePathForRename(string path, string newName);
        Task<string[]> getCleanRelativePathAndFileName(string path); //Radi slično kao gornja funkcija samo što odvojeno vraća file name i relativnu putanju. 
        Task deleteFile(string path);
        Task deleteFolder(string path);
        Task renameFile(string path, string newName);
        Task renameFolder(string path, string newName);
        Task<string> getFileName(string path);
        Task<string> moveFileForCheck(string path, bool fileAlreadyChecked);
    }
}
