using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace TheProppyAPI.Helpers
{
    public class AWs3Services
    {
        string AccessKey= new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AWS")["AWSAccessKeyID"];
        string SecretKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AWS")["AWSSecretAccessKey"];
        string S3URL = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AWSFolders")["Url"];
        string BucketName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AWS")["BucketName"];
        public string GetFile(string FileName)
        {
            return "https://" + BucketName + S3URL + "/" + FileName;
        }
        //public async Task<string> DeleteFileFromS3(string bucketName, string FileName)
        //{
        //    var bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
        //    if (!bucketExists) return $"Bucket {bucketName} does not exist";
        //    await _s3Client.DeleteObjectAsync(bucketName, FileName);
        //    return "Deleted";
        //}
        public async Task<string> UploadFileInS3(IFormFile file,string ImageName)
        {
            var credentials = new BasicAWSCredentials(AccessKey, SecretKey);
            var config = new AmazonS3Config()
            {
                RegionEndpoint = Amazon.RegionEndpoint.USEast1
            };
            // Process file
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var response = new S3ResponseDto();
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest()
                {
                    InputStream = memoryStream,
                    Key = ImageName,
                    BucketName = BucketName,
                    //CannedACL = S3CannedACL.NoACL
                };

                // initialise client
                using var client = new AmazonS3Client(credentials, config);

                // initialise the transfer/upload tools
                var transferUtility = new TransferUtility(client);

                // initiate the file upload
                await transferUtility.UploadAsync(uploadRequest);

                response.StatusCode = 201;
                response.Message = "file has been uploaded sucessfully";
            }
            catch (AmazonS3Exception s3Ex)
            {
                response.StatusCode = (int)s3Ex.StatusCode;
                response.Message = s3Ex.Message;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return ("File uploaded to S3 successfully!");
        }
    }
    public class S3Object
    {
        public string Name { get; set; } = null!;
        public MemoryStream InputStream { get; set; } = null!;
        public string BucketName { get; set; } = null!;
        public string PresignedUrl { get; set; }
    }
    public class S3ResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
