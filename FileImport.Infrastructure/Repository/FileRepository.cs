using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using FileImport.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileImport.Infrastructure.Repository
{
    public class FileRepository : IFileRepository
    {
        private readonly IMemoryCache _cache;
        private readonly IConcurrentDictionaryRepository _allKeys;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1); //Ovaj repository je singleton i mora da bude thread safe.
        public FileRepository(IMemoryCache cache, IConcurrentDictionaryRepository allKeys)
        {
            _cache = cache;
            _allKeys = allKeys;
        }
        public async Task WriteFilesToCache(string key, List<FileDetailsDto> files)
        {
            await _lock.WaitAsync();
            try
            {
                var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(180)); //Nakon 180 minuta u RAM-u briše se zapis o folderu.
                options.RegisterPostEvictionCallback(async (evictedKey, value, reason, state) =>
                {
                    //Ne koristimo await jer je callback, koristimo getawaiter i getresult da bi bilo thread safe.
                    _allKeys.Remove(evictedKey.ToString()!).GetAwaiter().GetResult(); //Brišemo i iz ove liste kada istekne keš. Ovo je najbolji način za postizanje toga.
                });
                await _allKeys.Add(key); //Lista potrebna za mass delete po ključu ili mass rename.
                _cache.Set(key, files, options);
            } finally
            {
                _lock.Release();
            }
        }
        public async Task<bool> IsKeyMapped(string key)
        {
            return _cache.TryGetValue(key, out _);
        }
        public async Task<bool> DoesFolderHasSubFolders(string key)
        {
            if (_cache.TryGetValue(key, out List<FileDetailsDto>? files))
            {
                if (files == null) return false;
                if (files.Where(x => x.isFolder).Count() == 0) return false;
                return true;
            }
            return false;
        }
        public async Task<List<FileDetailsDto>> GetFolderSubFolders(string key)
        {
            if (_cache.TryGetValue(key, out List<FileDetailsDto>? files))
            {
                if (files == null) return new List<FileDetailsDto>();
                files = files.Where(x => x.isFolder).ToList();
            }
            return files == null ? new List<FileDetailsDto>() : files; //null neće biti nikad ali neka ga.
        }
        public async Task<List<FileDetailsDto>> getFilesFromKey(string key)
        {
            if (_cache.TryGetValue(key, out List<FileDetailsDto>? files))
            {
                if (files == null) return new List<FileDetailsDto>();
                files = files.ToList();
            }
            return files == null ? new List<FileDetailsDto>() : files; //null bi bilo kad bi trygetvalue vratio da ne postoji pod ključom što se neće dešavati zbog validatora ali nije na odmet provera.
        }
        public async Task DeleteKey(string key, bool recursive) //Koristi se isključivo za foldere, nikada fajlove.
        {
            await _lock.WaitAsync();
            try
            {
                _cache.Remove(key);
                await _allKeys.Remove(key);
                //Argument recursive ako je true treba da pristupi listi svih ključeva koji su hijerarhijski ispod key-a i obriše ih.
                if (recursive)
                {
                    var keys = await _allKeys.GetSubKeys(string.Concat(key, "\\"));
                    foreach (var x in keys)
                    {
                        _cache.Remove(x);
                        await _allKeys.Remove(x);
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }
        public async Task DeleteFile(string fileName, string pathKey)
        {
            await _lock.WaitAsync();
            try
            {
                if (_cache.TryGetValue(pathKey, out List<FileDetailsDto>? files) && files != null)
                {
                    // Pravimo kopiju liste da ne bismo menjali original dok neko drugi možda čita
                    var newList = new List<FileDetailsDto>(files);
                    var toRemove = newList.FirstOrDefault(x => x.name == fileName);
                    if (toRemove != null)
                    {
                        newList.Remove(toRemove);
                        _cache.Set(pathKey, newList);
                    }
                }
            }
            finally { _lock.Release(); }
        }
        public async Task RenameKey(string key, string newKey, bool recursive)
        {
            if (_cache.TryGetValue(key, out List<FileDetailsDto>? value))
            {
                var copy = value?.ToList(); // Bezbedna kopija
                await this.DeleteKey(key, false); //Ne može da bude recursive jer nemamo info o nizovima u podfolderima.
                await this.WriteFilesToCache(newKey, copy != null ? copy : new List<FileDetailsDto>()); //Ne bi trebalo da value može da bude null al dobro.
            }
            if (recursive)
            {
                var keys = await _allKeys.GetSubKeys(string.Concat(key, "\\"));
                foreach (var x in keys)
                {
                    if (_cache.TryGetValue(x, out List<FileDetailsDto>? valueX))
                    {
                        var copyX = valueX?.ToList();
                        await this.DeleteKey(x, false); //Ne može da bude recursive jer nemamo info o nizovima u podfolderima.
                        //Ovde postoji potencijalni problem trebalo bi napraviti replacefirstonly.
                        //Zašto ovde ide replaceFirst? DOVRŠI...
                        //Zamislite da je neki od ključeva ovakav test\\folder\\test\\folder\\test\\folder a da smo izvorno krenuli da menjamo test\\folder, ne moram dalje da pišem šta bi replace uradio.
                        await this.WriteFilesToCache(x.ReplaceFirst(key, newKey), copyX != null ? copyX : new List<FileDetailsDto>()); //Ne bi trebalo da value može da bude null al dobro.
                    }
                }
            }
        }
        public async Task RenameFile(string fileName, string pathKey, string newName)
        {
            await _lock.WaitAsync();
            try
            {
                if (_cache.TryGetValue(pathKey, out List<FileDetailsDto>? files) && files != null)
                {
                    var newList = new List<FileDetailsDto>(files);
                    var index = newList.FindIndex(x => x.name == fileName);
                    if (index != -1)
                    {
                        // Kreiramo NOVU instancu DTO-a umesto modifikacije postojeće
                        var oldFile = newList[index];
                        newList[index] = new FileDetailsDto
                        {
                            name = oldFile.name,
                            isFolder = oldFile.isFolder,
                        };
                        _cache.Set(pathKey, newList);
                    }
                }
            }
            finally { _lock.Release(); }
        }
    }
}
