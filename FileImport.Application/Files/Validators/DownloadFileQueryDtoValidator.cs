using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class DownloadFileQueryDtoValidator : AbstractValidator<DownloadFileQueryDto>
    {
        private readonly IDownloadRepository _downloadRepository;
        public DownloadFileQueryDtoValidator(IDownloadRepository downloadRepository)
        {
            _downloadRepository = downloadRepository;
            RuleFor(x => x.key).NotEmpty().WithMessage("Morate uneti ključ za preuzimanje.").MustAsync((x, cancellation) => doesKeyExists(x)).WithMessage("Dati ključ ne postoji.");
        }
        private async Task<bool> doesKeyExists(string key)
        {
            var result = await this._downloadRepository.checkIfKeyExists(key);
            return result;
        }
    }
}
