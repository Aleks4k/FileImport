using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class RenameFileDtoValidator : AbstractValidator<RenameFileDto>
    {
        private readonly IFileService _fileService;
        public RenameFileDtoValidator(IFileService fileService)
        {
            _fileService = fileService;
            RuleFor(x => x.path).NotEmpty().WithMessage("Morate uneti putanju.").MustAsync((x, cancellation) => doesFileExists(x)).WithMessage("Dati fajl ne postoji.");
            RuleFor(x => x.newName).NotEmpty().WithMessage("Morate uneti novi naziv fajla."); //Ime fajla proveravamo u proveri ispod.
            RuleFor(x => x).MustAsync((x, cancellation) => isNewFileLegit(x)).WithMessage("Dati fajl vec postoji ili ime nije u dobrom formatu.");
        }
        private async Task<bool> doesFileExists(string path)
        {
            var result = await this._fileService.checkIfFileExists(path);
            return result;
        }
        private async Task<bool> isNewFileLegit(RenameFileDto dto)
        {
            var result = await this._fileService.checkNewFile(dto.path, dto.newName);
            return result;
        }
    }
}
