using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class GenerateRouteDtoValidator : AbstractValidator<GenerateRouteDto>
    {
        private readonly IFileService _fileService;
        private readonly IFileRepository _fileRepository;
        public GenerateRouteDtoValidator(IFileService fileService, IFileRepository fileRepository)
        {
            _fileService = fileService;
            _fileRepository = fileRepository;
            RuleFor(x => x.path).NotEmpty().WithMessage("Morate uneti putanju za generisanje.").MustAsync((x, cancellation) => isDirectoryAvailable(x)).WithMessage("Data putanja ne postoji.").MustAsync((x, cancellation) => isDirectoryNotMapped(x)).WithMessage("DIR_AL_LOADED"); //DIR_AL_LOADED je message code koji ćemo koristi na frontu.
        }
        private async Task<bool> isDirectoryAvailable(string path)
        {
            var result = await this._fileService.checkIfDirectoryExists(path);
            return result;
        }
        private async Task<bool> isDirectoryNotMapped(string path)
        {
            //Ako nije mapiran u fileRepository - puštamo request odnosno idemo dalje na mapiranje.
            var key = await this._fileService.getCleanRelativePath(path);
            var result = await _fileRepository.IsKeyMapped(key);
            return !result; //Vraćamo obrnuto zato što ako je mapirano validacija treba da padne.
        }
    }
}
