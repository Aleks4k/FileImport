using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class GetFolderContentDtoValidator : AbstractValidator<GetFolderContentDto>
    {
        private readonly IFileService _fileService;
        private readonly IFileRepository _fileRepository;
        public GetFolderContentDtoValidator(IFileService fileService, IFileRepository fileRepository)
        {
            _fileService = fileService;
            _fileRepository = fileRepository;
            //Ovde štitimo samo Path ostala dva argumenta nisu vredna pažnje.
            RuleFor(x => x.path).MustAsync((x, cancellation) => isDirectoryAvailable(x!)).WithMessage("Data putanja ne postoji.").MustAsync((x, cancellation) => isDirectoryMapped(x!)).WithMessage("DIR_NOT_LOADED");
        }
        private async Task<bool> isDirectoryAvailable(string path)
        {
            if(string.IsNullOrWhiteSpace(path)) return true; //Ukoliko je na ovaj poziv poslat root folder, treba da omogućimo da validacija prođe pošto bi donja funkcija za taj poziv vratila false zbog validacija vezanih za generisanje rute.
            var result = await this._fileService.checkIfDirectoryExists(path);
            return result;
        }
        private async Task<bool> isDirectoryMapped(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return true; //Ukoliko je na ovaj poziv poslat root folder, treba da omogućimo da validacija prođe pošto bi donja funkcija za taj poziv vratila false zbog validacija vezanih za generisanje rute.
            //Ako nije mapiran u fileRepository - puštamo request odnosno idemo dalje na mapiranje.
            var key = await this._fileService.getCleanRelativePath(path);
            var result = await _fileRepository.IsKeyMapped(key);
            return result;
        }
    }
}
