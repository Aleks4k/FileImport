using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class DoesFolderHasSubFoldersDtoValidator : AbstractValidator<DoesFolderHasSubFoldersDto>
    {
        private readonly IFileService _fileService;
        public DoesFolderHasSubFoldersDtoValidator(IFileService fileService)
        {
            _fileService = fileService;
            RuleFor(x => x.path).NotEmpty().WithMessage("Morate uneti putanju.").MustAsync((x, cancellation) => isDirectoryAvailable(x)).WithMessage("Data putanja ne postoji.");
        }
        private async Task<bool> isDirectoryAvailable(string path)
        {
            var result = await this._fileService.checkIfDirectoryExists(path);
            return result;
        }
    }
}
