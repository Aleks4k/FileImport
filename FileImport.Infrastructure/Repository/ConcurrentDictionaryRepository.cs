using FileImport.Application.Files.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Infrastructure.Repository
{
    public class ConcurrentDictionaryRepository : IConcurrentDictionaryRepository
    {
        private readonly ConcurrentDictionary<string, byte> _allKeys = new();
        public ConcurrentDictionaryRepository(){}
        public async Task Remove(string key)
        {
            _allKeys.TryRemove(key, out _);
        }
        public async Task Add(string key)
        {
            _allKeys.TryAdd(key, 0);
        }
        public async Task<List<string>> GetSubKeys(string key)
        {
            //Ovoj metodi moramo da obezbedimo normalan ulaz jer npr. situacija.
            //Imamo foldere paket1\\hello i paket1\\hello2 i metoda dobija argument paket1\\hello, bez \\ će ona kao svoj subkey da vrati i folder paket1\\hello2 a to ne bi smelo jer taj folder nije subfolder ovog foldera.
            if (!key.EndsWith("\\")) throw new ArgumentException("Method GetSubKeys expect \\ at end.");
            return _allKeys.Keys.Where(k => k.StartsWith(key)).ToList();
        }
    }
}
