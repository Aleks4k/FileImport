using Azure;
using Azure.Core;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using FileImport.Domain.Exceptions;
using FileImport.Infrastructure.Extensions;
using FileImport.Infrastructure.Settings;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Text;

namespace FileImport.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly FileStorageOptions _options;
        public FileService(IOptions<FileStorageOptions> options)
        {
            _options = options.Value;
        }
        public async Task<RouteDetailsDto> getAllFilesFromRootPath()
        {
            var response = new RouteDetailsDto();
            string fullPath = Path.GetFullPath(_options.RootPath);
            if (!Directory.Exists(fullPath)) throw new InvalidFilePathException("Non-existent root folder.");
            string[] directories = Directory.GetDirectories(fullPath);
            Array.Sort(directories);
            //Array.Sort(directories, (a, b) => string.Compare(b, a));
            string[] files = Directory.GetFiles(fullPath);
            Array.Sort(files);
            //Array.Sort(files, (a, b) => string.Compare(b, a));
            response.path = string.Empty; //Mapiramo root folder te je logično da je relativna putanja prazan string.
            foreach (string d in directories)
            {
                response.files.Add(new FileDetailsDto
                {
                    name = Path.GetFileName(d),
                    isFolder = true
                });
            }
            foreach (string f in files)
            {
                response.files.Add(new FileDetailsDto
                {
                    name = Path.GetFileName(f),
                    isFolder = false
                });
            }
            return response;
        }
        public async Task<RouteDetailsDto> getAllFilesFromPath(string path)
        {
            var response = new RouteDetailsDto();
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            //Ove tri provere ispod već postoje na checkIfDirectoryExists ali su tu za svaki slučaj future-proof je malo više uz njih.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not mappable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid folder path."); //Sprečavamo traversal napade.
            if (!Directory.Exists(fullPath)) throw new InvalidFilePathException("Non-existent folder."); //Ovde će uvek da postoji jer validator komande koja poziva ovaj poziv bi već trebalo da ima proveru za ovo kroz funkciju checkIfDirectoryExists.
            string[] directories = Directory.GetDirectories(fullPath);
            Array.Sort(directories);
            string[] files = Directory.GetFiles(fullPath);
            Array.Sort(files);
            response.path = path = Path.GetRelativePath(_options.RootPath, fullPath); //Ovo je sada normalizovana vrednost za Path u slučaju da je napadač slao neke bljuvotine.
            foreach (string d in directories)
            {
                response.files.Add(new FileDetailsDto
                {
                    name = Path.GetFileName(d),
                    isFolder = true
                });
            }
            foreach (string f in files)
            {
                response.files.Add(new FileDetailsDto
                {
                    name = Path.GetFileName(f),
                    isFolder = false
                });
            }
            return response;
        }
        public async Task<string> getCleanRelativePathForRename(string path, string newName)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not mappable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid folder path."); //Sprečavamo traversal napade. Vraćamo samo false.
            var folderPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(folderPath)) throw new InvalidFilePathException("Invalid folder path.");
            string newFullPath = Path.GetFullPath(Path.Combine(folderPath, newName)); //Dobijamo novi safe path za novi fajl (u slučaju da je pokušao kroz newName da ubaci neku glupost.
            if (newFullPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid folder path."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!newFullPath.StartsWith(string.Concat(folderPath, "\\"), StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid folder path."); //Sprečavamo traversal napade. Vraćamo samo false.
            return Path.GetRelativePath(_options.RootPath, newFullPath);
        }
        public async Task<string> getCleanRelativePath(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not mappable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid folder path."); //Sprečavamo traversal napade. Vraćamo samo false.
            return Path.GetRelativePath(_options.RootPath, fullPath);
        }
        public async Task<string[]> getCleanRelativePathAndFileName(string path) //Ova funkcija radi i na folderima, vratiće ime foldera i relativno ime njegovog roditelja.
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not mappable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid folder path."); //Sprečavamo traversal napade. Vraćamo samo false.
            var fileName = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(fileName)) throw new InvalidFilePathException("Unknown file name.");
            var filePath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(filePath)) throw new InvalidFilePathException("Unknown path name.");
            var relativeFilePath = Path.GetRelativePath(_options.RootPath, filePath);
            return new string[] { fileName, relativeFilePath };
        }
        public async Task<bool> checkIfDirectoryExists(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Sprečavamo traversal napade. Vraćamo samo false.
            return Directory.Exists(fullPath);
        }
        public async Task<bool> checkIfFileExists(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Sprečavamo traversal napade. Vraćamo samo false.
            return File.Exists(fullPath);
        }
        public async Task<string> getFileName(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not uploadable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid file path."); //Sprečavamo traversal napade. Vraćamo samo false.
            if (!File.Exists(fullPath)) throw new InvalidFilePathException("File not found.");
            var fileName = Path.GetFileName(fullPath);
            if(string.IsNullOrEmpty(fileName)) throw new InvalidFilePathException("File name not found.");
            return fileName;
        }
        public async Task<bool> checkIfDirectoryHasSubFolders(string path)
        {
            //Ulaz u ovu funkciju je već čist ali je futureproof da za svaki slučaj opet obradimo.
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Sprečavamo traversal napade. Vraćamo samo false.
            string[] directories = Directory.GetDirectories(fullPath);
            if(directories.Length == 0) return false;
            return true;
        }
        public async Task<bool> checkIfDirectoryHasSubFiles(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Sprečavamo traversal napade. Vraćamo samo false.
            string[] files = Directory.GetFiles(fullPath);
            if (files.Length == 0) return false;
            return true;
        }
        public async Task<List<FileDetailsDto>> getDirectorySubFolders(string path)
        {
            var files = new List<FileDetailsDto>();
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return files; //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return files; //Sprečavamo traversal napade. Vraćamo samo false.
            string[] directories = Directory.GetDirectories(fullPath);
            Array.Sort(directories);
            foreach (string d in directories)
            {
                files.Add(new FileDetailsDto
                {
                    name = Path.GetFileName(d),
                    isFolder = true
                });
            }
            return files;
        }
        public async Task<DownloadFileResponseDto> getFileDownload(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not downloadable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid file path."); //Sprečavamo traversal napade. Vraćamo samo false.
            if (!File.Exists(fullPath)) throw new InvalidFilePathException("File not found.");
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            if(stream == null || stream == FileStream.Null) throw new InvalidFilePathException("File not readable.");
            return new DownloadFileResponseDto
            {
                FileStream = stream,
                FileName = Path.GetFileName(fullPath),
                ContentType = "application/octet-stream"
            };
        }
        public async Task deleteFile(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not downloadable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid file path."); //Sprečavamo traversal napade. Vraćamo samo false.
            if (!File.Exists(fullPath)) throw new InvalidFilePathException("File not found.");
            File.Delete(fullPath);
        }
        public async Task deleteFolder(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not downloadable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid folder path."); //Sprečavamo traversal napade. Vraćamo samo false.
            if (!Directory.Exists(fullPath)) throw new InvalidFilePathException("Folder not found.");
            Directory.Delete(fullPath, true);
        }
        public async Task<bool> checkNewFile(string path, string newName)
        {
            //Ova metoda ne sadrži proveru da li stari fajl postoji zato što se podrazumeva da je pre ove provere ta druga već pozvana.
            if (string.IsNullOrWhiteSpace(newName)) return false;
            if(newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;
            string extension = Path.GetExtension(newName);
            if (string.IsNullOrEmpty(extension) || extension.Length < 2) return false;
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Sprečavamo traversal napade. Vraćamo samo false.
            var filePath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(filePath)) return false;
            string newFullPath = Path.GetFullPath(Path.Combine(filePath, newName)); //Dobijamo novi safe path za novi fajl (u slučaju da je pokušao kroz newName da ubaci neku glupost.
            if (!newFullPath.StartsWith(string.Concat(filePath, "\\"), StringComparison.OrdinalIgnoreCase)) return false; //Sprečavamo traversal napade. Vraćamo samo false.
            return !File.Exists(newFullPath); //Ako fajl postoji ova validacija treba da padne zato !.
        }
        public async Task<bool> checkNewFolder(string path, string newName)
        {
            //Ova metoda ne sadrži proveru da li stari folder postoji zato što se podrazumeva da je pre ove provere ta druga već pozvana.
            if (string.IsNullOrWhiteSpace(newName)) return false;
            if (newName.IndexOfAny(Path.GetInvalidPathChars()) >= 0) return false;
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) return false; //Sprečavamo traversal napade. Vraćamo samo false.
            var folderPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(folderPath)) return false;
            string newFullPath = Path.GetFullPath(Path.Combine(folderPath, newName)); //Dobijamo novi safe path za novi fajl (u slučaju da je pokušao kroz newName da ubaci neku glupost.
            if (newFullPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase)) return false; //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!newFullPath.StartsWith(string.Concat(folderPath, "\\"), StringComparison.OrdinalIgnoreCase)) return false; //Sprečavamo traversal napade. Vraćamo samo false.
            return !Directory.Exists(newFullPath); //Ako folder postoji ova validacija treba da padne zato !.
        }
        public async Task renameFile(string path, string newName)
        {
            //Naravno da nismo morali opet sve validacije za newName i path ali je future proof da ih stavimo.
            if (string.IsNullOrWhiteSpace(newName)) throw new InvalidFilePathException("New file name is not in good format.");
            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidFilePathException("New file name is not in good format.");
            string extension = Path.GetExtension(newName);
            if (string.IsNullOrEmpty(extension) || extension.Length < 2) throw new InvalidFilePathException("New file name is not in good format.");
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not downloadable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid file path."); //Sprečavamo traversal napade. Vraćamo samo false.
            if (!File.Exists(fullPath)) throw new InvalidFilePathException("File not found.");
            var filePath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(filePath)) throw new InvalidFilePathException("Unknown path name.");
            string newFullPath = Path.GetFullPath(Path.Combine(filePath, newName)); //Dobijamo novi safe path za novi fajl (u slučaju da je pokušao kroz newName da ubaci neku glupost.
            if (File.Exists(newFullPath)) throw new InvalidFilePathException("File with that name already exists.");
            File.Move(fullPath, newFullPath);
        }
        public async Task renameFolder(string path, string newName)
        {
            //Naravno da nismo morali opet sve validacije za newName i path ali je future proof da ih stavimo.
            if (string.IsNullOrWhiteSpace(newName)) throw new InvalidFilePathException("New folder name is not in good format.");
            if (newName.IndexOfAny(Path.GetInvalidPathChars()) >= 0) throw new InvalidFilePathException("New folder name is not in good format.");
            string fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not renamable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(_options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid folder path."); //Sprečavamo traversal napade. Vraćamo samo false.
            if (!Directory.Exists(fullPath)) throw new InvalidFilePathException("Folder not found.");
            var folderPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(folderPath)) throw new InvalidFilePathException("Unknown path name.");
            string newFullPath = Path.GetFullPath(Path.Combine(folderPath, newName)); //Dobijamo novi safe path za novi fajl (u slučaju da je pokušao kroz newName da ubaci neku glupost.
            if (newFullPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not renamable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!newFullPath.StartsWith(string.Concat(folderPath, "\\"), StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not renamable."); //Sprečavamo traversal napade. Vraćamo samo false.
            if(Directory.Exists(newFullPath)) throw new InvalidFilePathException("Folder with that name already exists.");
            Directory.Move(fullPath, newFullPath);
        }
        public async Task<string> moveFileForCheck(string path, bool fileAlreadyChecked)
        {
            //Ova metoda služi da se premesti fajl u check directory ili ako je već u njemu onda ga vraćamo na staro mesto (to se radi ako pukne snimanje u sql).
            //Metoda vraća relativnu lokaciju gde je fajl premešten.
            if (string.IsNullOrEmpty(path)) throw new InvalidFilePathException("Path is required.");
            string fullPath = Path.GetFullPath(Path.Combine(fileAlreadyChecked ? _options.RootPathChecked : _options.RootPath, path)); //Sprečavamo traversal napade. Nakon ove linije ne sme da se koristi varijabla path.
            if (fullPath.Equals(fileAlreadyChecked ? _options.RootPathChecked : _options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Root path is not uploadable."); //Pokušao je sa \\.. da dođe i mapira root folder ne može.
            if (!fullPath.StartsWith(fileAlreadyChecked ? _options.RootPathChecked : _options.RootPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidFilePathException("Invalid file path."); //Sprečavamo traversal napade. Vraćamo samo false.
            if (!File.Exists(fullPath)) throw new InvalidFilePathException("File not found.");
            string newFullPath = fullPath.ReplaceFirst(fileAlreadyChecked ? _options.RootPathChecked : _options.RootPath, fileAlreadyChecked ? _options.RootPath : _options.RootPathChecked);
            if (File.Exists(newFullPath)) throw new InvalidFilePathException("File already exists.");
            var newDirectoryPath = Path.GetDirectoryName(newFullPath);
            Directory.CreateDirectory(newDirectoryPath!);
            File.Move(fullPath, newFullPath);
            return Path.GetRelativePath(fileAlreadyChecked ? _options.RootPath : _options.RootPathChecked, newFullPath);
        }
    }
}
