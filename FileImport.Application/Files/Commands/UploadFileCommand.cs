using FluentValidation;
using FileImport.Application.Common.Contracts;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using FileImport.Application.Files.Validators;
using FileImport.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FileImport.Application.Files.Commands
{
    public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        public UploadFileCommandValidator(IFileService fileService, IFileArchiveRepository fileArchiveRepository)
        {
            RuleFor(x => x.Request).SetValidator(new UploadFileDtoValidator(fileService, fileArchiveRepository));
        }
    }
    public class UploadFileCommand : IRequest<Unit>
    {
        public required UploadFileDto Request { get; set; }
        public UploadFileCommand(){}
        public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Unit>
        {
            private IFileArchiveRepository _fileArchiveRepository;
            private IFileService _fileService;
            private IHttpContextAccessor _httpContextAccessor;
            private IFileRepository _fileRepository;
            private readonly IJwtService _jwtService;
            public UploadFileCommandHandler(IFileArchiveRepository fileArchiveRepository, IFileService fileService, IHttpContextAccessor httpContextAccessor, IFileRepository fileRepository, IJwtService jwtService) { 
                _fileArchiveRepository = fileArchiveRepository;
                _fileService = fileService;
                _httpContextAccessor = httpContextAccessor;
                _fileRepository = fileRepository;
                _jwtService = jwtService;
            }
            public async Task<Unit> Handle(UploadFileCommand request, CancellationToken cancellationToken)
            {
                var authorizationHeader = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].FirstOrDefault();
                var token = authorizationHeader?.Substring("Bearer ".Length).Trim();
                var user_id = await _jwtService.GetUserIdFromAccessToken(token!);
                if (user_id == 0)
                {
                    throw new UnauthorizedAccessException("Access token is not valid.");
                }
                //Premeštamo fajl.
                var fileName = await _fileService.getFileName(request.Request.path); //Moramo ovo pre moveFile jer nismo adapitrali da getFileName radi sa checked folderom.
                var newFileLocation = await _fileService.moveFileForCheck(request.Request.path, false);
                var DocumentNumber = fileName.Split('_')[0];
                //Sada snimamo u repository.
                try {
                    await _fileArchiveRepository.saveCheckedFile(DocumentNumber, user_id, newFileLocation);
                }
                catch {
                    //Vraćamo fajl sa nove lokacije na staru.
                    _ = await _fileService.moveFileForCheck(newFileLocation, true);
                    throw;
                }
                //Ako sve prođe brišemo ga iz repository.
                var keys = await _fileService.getCleanRelativePathAndFileName(request.Request.path);
                await _fileRepository.DeleteFile(keys[0], keys[1]);
                return Unit.Value;
            }
        }
    }
}
