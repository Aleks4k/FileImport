using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using FileImport.Application.Files.Validators;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileImport.Application.Files.Queries
{
    public class DoesFolderHasSubFoldersQueryValidator : AbstractValidator<DoesFolderHasSubFoldersQuery>
    {
        public DoesFolderHasSubFoldersQueryValidator(IFileService fileService)
        {
            RuleFor(x => x.Request).SetValidator(new DoesFolderHasSubFoldersDtoValidator(fileService));
        }
    }
    public class DoesFolderHasSubFoldersQuery : IRequest<bool>
    {
        public required DoesFolderHasSubFoldersDto Request { get; set; }
        public DoesFolderHasSubFoldersQuery(){}
        public class DoesFolderHasSubFoldersQueryHandler : IRequestHandler<DoesFolderHasSubFoldersQuery, bool>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public DoesFolderHasSubFoldersQueryHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<bool> Handle(DoesFolderHasSubFoldersQuery request, CancellationToken cancellationToken)
            {
                var key = await this._fileService.getCleanRelativePath(request.Request.path);
                var result = await _fileRepo.IsKeyMapped(key);
                if (result)
                {
                    return await _fileRepo.DoesFolderHasSubFolders(key);
                }
                else
                {
                    return await _fileService.checkIfDirectoryHasSubFolders(key); //Ova funkcija duplo normalizuje ključ, nema veze :).
                }
            }
        }
    }
}
