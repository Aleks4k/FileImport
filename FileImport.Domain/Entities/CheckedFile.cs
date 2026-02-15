using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Domain.Entities
{
    public class CheckedFile
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public int AuthorizedUserId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public virtual AuthorizedUser? AuthorizedUser { get; set; }
    }
}
