namespace KariyerTakip.Services;

public class FileSystemManager
{
    public async Task UploadFile(IFormFile file, string folder, string filePath)
    {
        
            Directory.CreateDirectory(folder);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            } 
    }
}