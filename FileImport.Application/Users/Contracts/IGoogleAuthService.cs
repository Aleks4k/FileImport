using FileImport.Application.Users.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Users.Contracts
{
    public interface IGoogleAuthService
    {
        Task<bool> ValidateGoogleToken(string token);
        Task<GooglePayloadDto> GetGooglePayload(string token);
    }
}
