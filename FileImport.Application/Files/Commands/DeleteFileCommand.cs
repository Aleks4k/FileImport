using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using FileImport.Application.Files.Validators;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Commands
{
    public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
    {
        public DeleteFileCommandValidator(IFileService fileService)
        {
            RuleFor(x => x.Request).SetValidator(new DeleteFileDtoValidator(fileService));
        }
    }
    public class DeleteFileCommand : IRequest<Unit>
    {
        public required DeleteFileDto Request { get; set; }
        public DeleteFileCommand(){}
        public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Unit>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public DeleteFileCommandHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<Unit> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
            {
                //Prvo treba iz path-a da dobijemo ključ za repository i fileName odvojeno.
                var keys = await _fileService.getCleanRelativePathAndFileName(request.Request.path);
                await _fileRepo.DeleteFile(keys[0], keys[1]);
                await _fileService.deleteFile(request.Request.path);
                return Unit.Value;
            }
        }
    }
}
