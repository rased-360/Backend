using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces
{
    public interface ICloudinaryService
    {
        public Task<string> UploadImageAsync(IFormFile file, string folderName);
        public Task<bool> DeleteImageAsync(string publicId);
    }
}
