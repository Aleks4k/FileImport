using FileImport.Application.Users.Contracts;
using FileImport.Domain.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Infrastructure.Repository
{
    public class UserRepository : IUser
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<int> IsUserAuthorized(string mail)
        {
            //Ukoliko korisnik ne postoji dobićemo 0, u suprotnom dobijamo user_id.
            var result = await _context.AuthorizedUsers.AsNoTracking().Where(x => x.Email == mail).Select(x => x.Id).FirstOrDefaultAsync();
            return result;
        }
    }
}
