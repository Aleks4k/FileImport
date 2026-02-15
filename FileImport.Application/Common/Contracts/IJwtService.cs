using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace FileImport.Application.Common.Contracts
{
    public interface IJwtService
    {
        string GenerateAccessToken(List<Claim> claims);
        Task<int> GetUserIdFromAccessToken(string token);
    }
}
