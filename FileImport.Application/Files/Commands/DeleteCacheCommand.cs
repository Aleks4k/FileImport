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
    public class DeleteCacheCommandValidator : AbstractValidator<DeleteCacheCommand>
    {
        public DeleteCacheCommandValidator()
        {
            RuleFor(x => x.Request).SetValidator(new DeleteCacheDtoValidator());
        }
    }
    public class DeleteCacheCommand : IRequest<Unit>
    {
        public required DeleteCacheDto Request { get; set; }
        public DeleteCacheCommand(){}
        public class DeleteCacheCommandHandler : IRequestHandler<DeleteCacheCommand, Unit>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public DeleteCacheCommandHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<Unit> Handle(DeleteCacheCommand request, CancellationToken cancellationToken)
            {
                var key = await _fileService.getCleanRelativePath(request.Request.key);
                await _fileRepo.DeleteKey(key, false); //Ovde brišemo samo ovaj ključ iz keša, ako on ima subfoldere koji su već učitani nemamo razloga na ovom pozivu da ih obrišemo.
                return Unit.Value;
            }
        }
    }
}
