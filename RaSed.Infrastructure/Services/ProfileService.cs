using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using RaSed.Application.DTOs.Profile;
using RaSed.Application.Interfaces;
using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services
{
    public class ProfileService: IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICloudinaryService _cloudinaryService;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            ICloudinaryService cloudinaryService)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ProfileResponseDto> GetProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new Exception("User not found");

            var age = CalculateAge(user.DateOfBirth);

            return new ProfileResponseDto
            {
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender.ToString(),
                Age = age,
                PhoneNumber = user.PhoneNumber,
                NationalId = user.NationalId,
                HireType = user.HireType.ToString(),
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }

        public async Task<UpdateProfilePhotoResponseDto> UpdateProfilePhotoAsync(int userId, IFormFile photo)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new Exception("User not found");

            // Delete old photo if exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var publicId = ExtractPublicIdFromUrl(user.ProfilePictureUrl);
                await _cloudinaryService.DeleteImageAsync(publicId);
            }

            // Upload new photo
            var newPhotoUrl = await _cloudinaryService.UploadImageAsync(photo, "profile-pictures");

            // Update user
            user.ProfilePictureUrl = newPhotoUrl;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to update profile photo");

            return new UpdateProfilePhotoResponseDto
            {
                ProfilePictureUrl = newPhotoUrl,
                Message = "Profile photo updated successfully"
            };
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
                age--;

            return age;
        }

        private string ExtractPublicIdFromUrl(string url)
        {
            // Example URL: https://res.cloudinary.com/demo/image/upload/v1234567890/profile-pictures/abc123.jpg
            // Public ID: profile-pictures/abc123

            var uri = new Uri(url);
            var segments = uri.Segments;

            // Find "upload" segment and get everything after version number
            var uploadIndex = Array.FindIndex(segments, s => s.Contains("upload"));

            if (uploadIndex >= 0 && uploadIndex + 2 < segments.Length)
            {
                var pathAfterVersion = string.Join("", segments.Skip(uploadIndex + 2));
                // Remove file extension
                return Path.ChangeExtension(pathAfterVersion.TrimEnd('/'), null)?.TrimEnd('.');
            }

            return string.Empty;
        }
    }
}
