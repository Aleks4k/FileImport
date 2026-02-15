using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class DeleteFolderDtoValidator : AbstractValidator<DeleteFolderDto>
    {
        private readonly IFileService _fileService;
        public DeleteFolderDtoValidator(IFileService fileService)
        {
            _fileService = fileService;
            RuleFor(x => x.path).NotEmpty().WithMessage("Morate uneti putanju.").MustAsync((x, cancellation) => doesFolderExists(x)).WithMessage("Dati folder ne postoji."); ;
            RuleFor(x => x).MustAsync((x, cancellation) => isFolderEmpty(x)).WithMessage("FOLDER_NOT_EMPTY");
        }
        private async Task<bool> doesFolderExists(string path)
        {
            var result = await this._fileService.checkIfDirectoryExists(path);
            return result;
        }
        private async Task<bool> isFolderEmpty(DeleteFolderDto dto)
        {
            if (dto.notEmptyAgree) return true;
            var result = await this._fileService.checkIfDirectoryHasSubFolders(dto.path);
            if (result) return false;
            result = await this._fileService.checkIfDirectoryHasSubFiles(dto.path);
            return !result;
        }
    }
}
