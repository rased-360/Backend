using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class PasswordOperationResult
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public static PasswordOperationResult Success(string message) => new()
        {
            IsSuccessful = true,
            Message = message
        };

        public static PasswordOperationResult Failure(string error) => new()
        {
            IsSuccessful = false,
            Message = error,
            Errors = new List<string> { error }
        };

        public static PasswordOperationResult Failure(List<string> errors, string message = null) => new()
        {
            IsSuccessful = false,
            Message = message ?? "Operation failed",
            Errors = errors
        };
    }
}
