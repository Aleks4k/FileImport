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
    public class RequestDownloadQueryValidator : AbstractValidator<RequestDownloadQuery>
    {
        public RequestDownloadQueryValidator(IFileService fileService)
        {
            RuleFor(x => x.Request).SetValidator(new RequestDownloadQueryDtoValidator(fileService));
        }
    }
    public class RequestDownloadQuery : IRequest<string>
    {
        public required RequestDownloadQueryDto Request { get; set; }
        public RequestDownloadQuery(){}
        public class RequestDownloadQueryHandler : IRequestHandler<RequestDownloadQuery, string>
        {
            private readonly IFileService _fileService;
            private readonly IDownloadRepository _downloadRepository;
            public RequestDownloadQueryHandler(IFileService fileService, IDownloadRepository downloadRepository)
            {
                _fileService = fileService;
                _downloadRepository = downloadRepository;
            }
            public async Task<string> Handle(RequestDownloadQuery request, CancellationToken cancellationToken)
            {
                var key = await this._fileService.getCleanRelativePath(request.Request.path);
                return await _downloadRepository.GenerateGUID(key);
            }
        }
    }
}
