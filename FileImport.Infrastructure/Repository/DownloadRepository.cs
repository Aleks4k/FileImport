using FileImport.Application.Files.Contracts;
using FileImport.Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Infrastructure.Repository
{
    public class DownloadRepository : IDownloadRepository
    {
        private readonly IMemoryCache _cache;
        public DownloadRepository(IMemoryCache cache)
        {
            _cache = cache;
        }
        public async Task<bool> checkIfKeyExists(string key)
        {
            return _cache.TryGetValue(key, out _);
        }
        public async Task<string> GenerateGUID(string key)
        {
            string downloadToken = Guid.NewGuid().ToString();
            _cache.Set(downloadToken, key, TimeSpan.FromSeconds(60));
            return downloadToken;
        }

        public async Task<string> getFilePathFromKey(string key)
        {
            if (_cache.TryGetValue(key, out string? path))
            {
                if (path == null || string.IsNullOrEmpty(path)) throw new InvalidFilePathException("File key not found."); //Ne bi trebalo da se desi jer je provera već prošla.
                //Odmah brišemo key.
                _cache.Remove(key);
                return path;
            }
            throw new InvalidFilePathException("File key not found."); //Ne bi trebalo da se desi jer je provera već prošla.
        }
    }
}
