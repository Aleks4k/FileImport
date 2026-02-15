using FluentValidation;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Files.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Files.Validators
{
    public class DeleteCacheDtoValidator : AbstractValidator<DeleteCacheDto>
    {
        public DeleteCacheDtoValidator()
        {
            //Ovde ne koristimo isDirectoryMapped i isDirectoryAvailable je su nepotrebni i usporili bi ceo proces bez ikakvog razloga.
            RuleFor(x => x.key).NotEmpty().WithMessage("Morate uneti ključ za brisanje.");
        }
    }
}
