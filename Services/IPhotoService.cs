using CloudinaryDotNet.Actions;

namespace Aimachine.Services
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file, string folderName);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}