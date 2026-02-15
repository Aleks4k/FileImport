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
    public class GetFolderContentQueryValidator : AbstractValidator<GetFolderContentQuery>
    {
        public GetFolderContentQueryValidator(IFileService fileService, IFileRepository fileRepository)
        {
            RuleFor(x => x.Request).SetValidator(new GetFolderContentDtoValidator(fileService, fileRepository));
        }
    }
    public class GetFolderContentQuery : IRequest<RouteDetailsDto>
    {
        public required GetFolderContentDto Request { get; set; }
        public GetFolderContentQuery(){}
        public class GetFolderContentQueryHandler : IRequestHandler<GetFolderContentQuery, RouteDetailsDto>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public GetFolderContentQueryHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<RouteDetailsDto> Handle(GetFolderContentQuery request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.Request.path))
                {
                    //Vraćamo sadržaj root folder-a kroz service jer se on ne mapira u repository.
                    return await _fileService.getAllFilesFromRootPath();
                } 
                else
                {
                    var key = await _fileService.getCleanRelativePath(request.Request.path);
                    var result = await _fileRepo.getFilesFromKey(key);
                    return new RouteDetailsDto { path = key, files = result };
                }  
            }
        }
    }
}
