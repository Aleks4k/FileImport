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
    public class DeleteFolderCommandValidator : AbstractValidator<DeleteFolderCommand>
    {
        public DeleteFolderCommandValidator(IFileService fileService)
        {
            RuleFor(x => x.Request).SetValidator(new DeleteFolderDtoValidator(fileService));
        }
    }
    public class DeleteFolderCommand : IRequest<Unit>
    {
        public required DeleteFolderDto Request { get; set; }
        public DeleteFolderCommand(){}
        public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, Unit>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public DeleteFolderCommandHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<Unit> Handle(DeleteFolderCommand request, CancellationToken cancellationToken)
            {
                var keyForPath = await _fileService.getCleanRelativePath(request.Request.path); //Ovaj ključ ne mora da postoji u fileRepository.
                var keysForParent = await _fileService.getCleanRelativePathAndFileName(request.Request.path);
                if (!keysForParent[1].Equals(".")) //Ako je . znači da je folder koji se briše u root direktorijumu a root direktorijum se ne mapira na repository.
                {
                    //Ovo znači da folder koji se briše ima roditelja koji ima ključ u repository i moramo da uklonimo folder iz njega.
                    await _fileRepo.DeleteFile(keysForParent[0], keysForParent[1]);
                }
                await _fileRepo.DeleteKey(keyForPath, true); //Kada brišemo folder, jako je bitno obrisati i sve subfoldere koje on sadži i njihove dalje potomke zbog memorije. Zamislite rename funkciju koja se stalno poziva na parent folderu foldera koji ima 10 hiljada redova, u tom slučaju memorija bi mogla kroz DoS da se prepuni u roku od par sekundi.
                await _fileService.deleteFolder(request.Request.path);
                return Unit.Value;
            }
        }
    }
}
