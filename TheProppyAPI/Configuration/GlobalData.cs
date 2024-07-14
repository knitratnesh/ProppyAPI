namespace TheProppyAPI.Configuration
{
    public class GlobalData
    {
        public string DeleteImage(IWebHostEnvironment _webHostEnvironment, string folderName, string ImageName)
        {
            var ImagePath = Path.Combine(_webHostEnvironment.WebRootPath, folderName, ImageName);
            FileInfo file = new FileInfo(ImagePath);
            if (file.Exists)//check file exsit or not  
            {
                file.Delete();
                return "Failed to image";
            }
            return "Deleted";
        }
        public async Task<string> SaveImage(Guid Id, IFormFile? ImageFile, IWebHostEnvironment _webHostEnvironment, string folderName)
        {
            string ImageName = string.Empty;
            if (ImageFile != null)
            {
                ImageName = Id + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(ImageFile.FileName);
                var ImagePath = Path.Combine(_webHostEnvironment.WebRootPath, folderName, ImageName);
                using (var fileStream = new FileStream(ImagePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }
            }
            return ImageName;
        }
        public async Task<string> UpdateImage(IFormFile ImageFile, IWebHostEnvironment _webHostEnvironment, string folderName, string ImageName)
        {
            var ImagePath = Path.Combine(_webHostEnvironment.WebRootPath, folderName, ImageName);
            using (var fileStream = new FileStream(ImagePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(fileStream);
            }
            return ImageName;
        }
    }
}
