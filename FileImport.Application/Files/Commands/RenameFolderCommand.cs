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
    public class RenameFolderCommandValidator : AbstractValidator<RenameFolderCommand>
    {
        public RenameFolderCommandValidator(IFileService fileService)
        {
            RuleFor(x => x.Request).SetValidator(new RenameFolderDtoValidator(fileService));
        }
    }
    public class RenameFolderCommand : IRequest<Unit>
    {
        public required RenameFolderDto Request { get; set; }
        public RenameFolderCommand(){}
        public class RenameFolderCommandHandler : IRequestHandler<RenameFolderCommand, Unit>
        {
            private readonly IFileRepository _fileRepo;
            private readonly IFileService _fileService;
            public RenameFolderCommandHandler(IFileRepository fileRepository, IFileService fileService)
            {
                _fileRepo = fileRepository;
                _fileService = fileService;
            }
            public async Task<Unit> Handle(RenameFolderCommand request, CancellationToken cancellationToken)
            {
                var keyForPath = await _fileService.getCleanRelativePath(request.Request.path); //Ovaj ključ ne mora da postoji u fileRepository.
                var keysForParent = await _fileService.getCleanRelativePathAndFileName(request.Request.path);
                var newKeyForPath = await _fileService.getCleanRelativePathForRename(request.Request.path, request.Request.newName);
                if (!keysForParent[1].Equals(".")) //Ako je . znači da je folder koji se rename u root direktorijumu a root direktorijum se ne mapira na repository.
                {
                    //Ovo znači da folder koji se rename ima roditelja koji ima ključ u repository i moramo da rename folder u njemu.
                    await _fileRepo.RenameFile(keysForParent[0], keysForParent[1], request.Request.newName);
                }
                //Promeni ime u repository (dakle stari ključ se briše i insertuje novi sa svim podacima) i svim podfolderima.
                await _fileRepo.RenameKey(keyForPath, newKeyForPath, true);
                //Promeni ime kroz servis.
                await _fileService.renameFolder(request.Request.path, request.Request.newName);
                return Unit.Value;
            }
        }
    }
}
