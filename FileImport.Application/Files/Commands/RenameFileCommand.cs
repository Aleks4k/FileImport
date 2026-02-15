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
    public class RenameFileCommandValidator : AbstractValidator<RenameFileCommand>
    {
        public RenameFileCommandValidator(IFileService fileService)
        {
            RuleFor(x => x.Request).SetValidator(new RenameFileDtoValidator(fileService));
        }
    }
    public class RenameFileCommand : IRequest<Unit>
    {
        public required RenameFileDto Request { get; set; }
        public RenameFileCommand(){}
        public class RenameFileCommandHandler : IRequestHandler<RenameFileCommand, Unit>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public RenameFileCommandHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<Unit> Handle(RenameFileCommand request, CancellationToken cancellationToken)
            {
                //Prvo treba iz path-a da dobijemo ključ za repository i fileName odvojeno.
                var keys = await _fileService.getCleanRelativePathAndFileName(request.Request.path);
                await _fileRepo.RenameFile(keys[0], keys[1], request.Request.newName);
                await _fileService.renameFile(request.Request.path, request.Request.newName);
                return Unit.Value;
            }
        }
    }
}
