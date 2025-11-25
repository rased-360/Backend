using Microsoft.AspNetCore.Http;
using RaSed.Application.DTOs.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileResponseDto> GetProfileAsync(int userId);
        Task<UpdateProfilePhotoResponseDto> UpdateProfilePhotoAsync(int userId, IFormFile photo);
    }
}
