using Google.Apis.Auth;
using FileImport.Application.Users.Contracts;
using FileImport.Application.Users.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace FileImport.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly ValidationSettings _googleSettings;
        public GoogleAuthService(ValidationSettings googleSettings)
        {
            _googleSettings = googleSettings;
        }
        public async Task<GooglePayloadDto> GetGooglePayload(string token)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, _googleSettings);
            //payload neće biti null jer smo već proverili na validacijama da li je dobar, čak iako se desi metoda ValidateAsync baca exception. 
            return new GooglePayloadDto { Email = payload.Email };
        }
        public async Task<bool> ValidateGoogleToken(string token)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token, _googleSettings);
                return true;
            }
            catch (InvalidJwtException)
            {
                return false;
            }
        }
    }
}
