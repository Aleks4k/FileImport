using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using FileImport.Application.Files.Validators;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Queries
{
    public class GetFolderSubFoldersQueryValidator : AbstractValidator<GetFolderSubFoldersQuery>
    {
        public GetFolderSubFoldersQueryValidator(IFileService fileService)
        {
            RuleFor(x => x.Request).SetValidator(new GetFolderSubFoldersDtoValidator(fileService));
        }
    }
    public class GetFolderSubFoldersQuery : IRequest<List<FileDetailsDto>>
    {
        public required GetFolderSubFoldersDto Request { get; set; }
        public GetFolderSubFoldersQuery(){}
        public class GetFolderSubFoldersQueryHandler : IRequestHandler<GetFolderSubFoldersQuery, List<FileDetailsDto>>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public GetFolderSubFoldersQueryHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<List<FileDetailsDto>> Handle(GetFolderSubFoldersQuery request, CancellationToken cancellationToken)
            {
                var key = await this._fileService.getCleanRelativePath(request.Request.path);
                var result = await _fileRepo.IsKeyMapped(key);
                if (result)
                {
                    return await _fileRepo.GetFolderSubFolders(key);
                } else
                {
                    return await _fileService.getDirectorySubFolders(key); //Ova funkcija duplo normalizuje ključ, nema veze :).
                }
            }
        }
    }
}
