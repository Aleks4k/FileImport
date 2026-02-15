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
    public class RouteGenerationCommandValidator : AbstractValidator<RouteGenerationCommand>
    {
        public RouteGenerationCommandValidator(IFileService fileService, IFileRepository fileRepository)
        {
            RuleFor(x => x.Route).SetValidator(new GenerateRouteDtoValidator(fileService, fileRepository));
        }
    }
    public class RouteGenerationCommand : IRequest<Unit>
    {
        public required GenerateRouteDto Route { get; set; }
        public RouteGenerationCommand(){}
        public class RouteGenerationCommandHandler : IRequestHandler<RouteGenerationCommand, Unit>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public RouteGenerationCommandHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<Unit> Handle(RouteGenerationCommand request, CancellationToken cancellationToken)
            {
                var response = await _fileService.getAllFilesFromPath(request.Route.path);
                //response.path je sada pravi path koji je normalizovan a ne request.Route.path on se ne koristi ispod ove linije.
                await _fileRepo.WriteFilesToCache(response.path, response.files);
                return Unit.Value;
            }
        }
    }
}
