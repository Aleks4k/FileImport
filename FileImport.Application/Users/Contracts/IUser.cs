using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Application.Users.Contracts
{
    public interface IUser
    {
        Task<int> IsUserAuthorized(string mail);
    }
}
