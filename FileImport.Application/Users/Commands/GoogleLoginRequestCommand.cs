using FluentValidation;
using FileImport.Application.Common.Contracts;
using FileImport.Application.Users.Contracts;
using FileImport.Application.Users.DTO;
using FileImport.Application.Users.Validators;
using MediatR;
using System.Security.Claims;

namespace FileImport.Application.Users.Commands
{
    public class GoogleLoginRequestCommandValidator : AbstractValidator<GoogleLoginRequestCommand>
    {
        public GoogleLoginRequestCommandValidator(IGoogleAuthService googleAuthService)
        {
            RuleFor(x => x.User).SetValidator(new GoogleLoginRequestDtoValidator(googleAuthService));
        }
    }
    public class GoogleLoginRequestCommand : IRequest<UserDetailsDto>
    {
        public required GoogleLoginRequestDto User { get; set; }
        public GoogleLoginRequestCommand(){}
        public class GoogleLoginRequestCommandHandler : IRequestHandler<GoogleLoginRequestCommand, UserDetailsDto>
        {
            private readonly IGoogleAuthService _googleAuthService;
            private readonly IJwtService _jwtService;
            private readonly IUser _userRepo;
            public GoogleLoginRequestCommandHandler(IGoogleAuthService googleAuthService, IJwtService jwtService, IUser userRepository)
            {
                _googleAuthService = googleAuthService;
                _jwtService = jwtService;
                _userRepo = userRepository;
            }
            public async Task<UserDetailsDto> Handle(GoogleLoginRequestCommand request, CancellationToken cancellationToken)
            {
                //Prvo proveravamo da li je email na whitelist-u. Ovo nismo uradili na validaciji jer ćemo ovde svakako morati da izvučemo ID.
                var payload = await _googleAuthService.GetGooglePayload(request.User.Token);
                var user_id = await _userRepo.IsUserAuthorized(payload.Email);
                if(user_id == 0) throw new UnauthorizedAccessException("You are not whitelisted.");
                UserDetailsDto result = new UserDetailsDto();
                var claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.NameIdentifier, user_id.ToString())
                        };
                result.access = _jwtService.GenerateAccessToken(claims);
                return result;
            }
        }
    }
}
