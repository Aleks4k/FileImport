using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using FileImport.Domain.Data;
using FileImport.Domain.Entities;
using FileImport.Domain.Exceptions;
using FileImport.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FileImport.Infrastructure.Repository
{
    public class FileArchiveRepository : IFileArchiveRepository
    {
        private readonly AppDbContext _context;
        public FileArchiveRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> checkFileNameFormat(string fileName)
        {
            if(string.IsNullOrEmpty(fileName)) return false;
            string pattern = @"^[0-9]{6}_.+\.[a-zA-Z0-9]+$";
            return Regex.IsMatch(fileName, pattern);
        }
        public async Task saveCheckedFile(string DocumentNumber, int user_id, string newFilePath)
        {
            //CheckedFile.DocumentNumber treba da bude indeksiran da bi ovaj upit prolazio brzo.
            var cfID = await _context.CheckedFiles.AsNoTracking().Where(x => x.DocumentNumber == DocumentNumber && x.FilePath == newFilePath).Select(x => x.Id).FirstOrDefaultAsync();
            if (cfID != 0) throw new Exception("This file is already checked.");
            CheckedFile entity = new CheckedFile { 
                AuthorizedUserId = user_id,
                FilePath = newFilePath,
                DocumentNumber = DocumentNumber
            };
            await _context.CheckedFiles.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
    }
}
