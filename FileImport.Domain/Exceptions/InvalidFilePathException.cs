using System;
using System.Collections.Generic;
using System.Text;

namespace FileImport.Domain.Exceptions
{
    public class InvalidFilePathException : Exception
    {
        public InvalidFilePathException(string message) : base(message)
        {
            
        }
    }
}
