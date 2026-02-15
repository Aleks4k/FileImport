using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class RenameFolderDtoValidator : AbstractValidator<RenameFolderDto>
    {
        private readonly IFileService _fileService;
        public RenameFolderDtoValidator(IFileService fileService)
        {
            _fileService = fileService;
            RuleFor(x => x.path).NotEmpty().WithMessage("Morate uneti putanju.").MustAsync((x, cancellation) => doesFolderExists(x)).WithMessage("Dati folder ne postoji."); ;
            RuleFor(x => x.newName).NotEmpty().WithMessage("Morate uneti novi naziv foldera."); //Ime foldera proveravamo u proveri ispod.
            RuleFor(x => x).MustAsync((x, cancellation) => isNewFolderLegit(x)).WithMessage("Dati folder vec postoji ili ime nije u dobrom formatu.");
        }
        private async Task<bool> doesFolderExists(string path)
        {
            var result = await this._fileService.checkIfDirectoryExists(path);
            return result;
        }
        private async Task<bool> isNewFolderLegit(RenameFolderDto dto)
        {
            var result = await this._fileService.checkNewFolder(dto.path, dto.newName);
            return result;
        }
    }
}
