using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class UploadFileDtoValidator : AbstractValidator<UploadFileDto>
    {
        private readonly IFileService _fileService;
        private readonly IFileArchiveRepository _fileArchiveRepository;
        public UploadFileDtoValidator(IFileService fileService, IFileArchiveRepository fileArchiveRepository)
        {
            _fileService = fileService;
            _fileArchiveRepository = fileArchiveRepository;
            RuleFor(x => x.path).NotEmpty().WithMessage("Morate uneti putanju.").MustAsync((x, cancellation) => doesFileExists(x)).WithMessage("Dati fajl ne postoji.").MustAsync((x, cancellation) => isFileNameValid(x)).WithMessage("File name is not in valid format {DocumentNumber}_{name}.{extension}. Document number must be exactly 6 characters long.");
        }
        private async Task<bool> doesFileExists(string path)
        {
            var result = await this._fileService.checkIfFileExists(path);
            return result;
        }
        private async Task<bool> isFileNameValid(string path)
        {
            var fileName = await this._fileService.getFileName(path);
            var fileNameValid = await this._fileArchiveRepository.checkFileNameFormat(fileName);
            return fileNameValid;
        }
    }
}
