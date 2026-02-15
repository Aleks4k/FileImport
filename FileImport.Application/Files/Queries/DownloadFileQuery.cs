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
    public class DownloadFileQueryValidator : AbstractValidator<DownloadFileQuery>
    {
        public DownloadFileQueryValidator(IDownloadRepository downloadRepository)
        {
            RuleFor(x => x.Request).SetValidator(new DownloadFileQueryDtoValidator(downloadRepository));
        }
    }
    public class DownloadFileQuery : IRequest<DownloadFileResponseDto>
    {
        public required DownloadFileQueryDto Request { get; set; }
        public DownloadFileQuery(){}
        public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, DownloadFileResponseDto>
        {
            private readonly IFileService _fileService;
            private readonly IDownloadRepository _downloadRepository;
            public DownloadFileQueryHandler(IFileService fileService, IDownloadRepository downloadRepository)
            {
                _fileService = fileService;
                _downloadRepository = downloadRepository;
            }
            public async Task<DownloadFileResponseDto> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
            {
                var path = await this._downloadRepository.getFilePathFromKey(request.Request.key);
                return await _fileService.getFileDownload(path);
            }
        }
    }
}
