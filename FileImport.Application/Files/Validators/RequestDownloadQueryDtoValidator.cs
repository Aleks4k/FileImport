using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class RequestDownloadQueryDtoValidator : AbstractValidator<RequestDownloadQueryDto>
    {
        private readonly IFileService _fileService;
        public RequestDownloadQueryDtoValidator(IFileService fileService)
        {
            _fileService = fileService;
            RuleFor(x => x.path).NotEmpty().WithMessage("Morate uneti putanju.").MustAsync((x, cancellation) => doesFileExists(x)).WithMessage("Dati fajl ne postoji.");
        }
        private async Task<bool> doesFileExists(string path)
        {
            var result = await this._fileService.checkIfFileExists(path);
            return result;
        }
    }
}
