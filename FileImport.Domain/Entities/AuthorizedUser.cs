using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Domain.Entities
{
    public class AuthorizedUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public virtual ICollection<CheckedFile> CheckedFiles { get; set; } = new List<CheckedFile>();
    }
}
