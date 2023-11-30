using Akka.Event;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Samurai.Integration.Application.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public class BlobService
    {
        public enum BlobFileType {
            images,
            videos,
            documents,
            others
        }

        private readonly IConfiguration _configuration;
        protected ILoggingAdapter _logger;
        private readonly IServiceProvider _serviceProvider;
        private CloudBlobClient _client;
        private const string BlobContainerName = "samurai-integration";

        public BlobService(IServiceProvider serviceProvider, IConfiguration configuration)
        { 
            _configuration = configuration;         
            _serviceProvider = serviceProvider;          

            _configuration = _serviceProvider.GetService<IConfiguration>();
            var storageConnectionString = _configuration.GetSection("AzureWebJobsStorage").Value;

            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                _client = storageAccount.CreateCloudBlobClient();
            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                throw new Exception(@"A connection string has not been defined in the system environment variables.
                                  Add an environment variable named 'AZURE_STORAGE_CONNECTION_STRING' with your storage connection string as a value.");
            }
        }

        public void Init(ILoggingAdapter logger)
        {
            _logger = logger;
        }

        public async Task<(bool success, string path)> UploadFileBlob(BlobFileType blobFileType, string fileName, string base64String, string rootPath, string preffix)
        {
            try
            {
                var cloudBlobContainer = await CreateBlobContainerIfNotExistsAsync();

                CloudBlockBlob cblob = cloudBlobContainer.GetBlockBlobReference($"{Path.Combine(blobFileType.ToString(), rootPath, preffix, fileName)}");
                var bytes = Convert.FromBase64String(base64String);// without data:image/jpeg;base64 prefix, just base64 string

                var stream = new MemoryStream(bytes);

                if (blobFileType == BlobFileType.images)
                {
                    cblob.Properties.ContentType = $"image/{fileName.Split(".").LastOrDefault() ?? "jpeg"}";

                    var image = Image.FromStream(new MemoryStream(bytes));

                    var maxResolution = new Size(3840, 2160); //3840 x 2160 pixel(4K UHDTV) resolution
                    if (image.Size.Width > maxResolution.Width || image.Size.Height > maxResolution.Height)
                    {
                        //var bitmap = new Bitmap(image).ResizekeepAspectRatio(70);
                        var bitmap = image.ResizeImage(70);
                        using var imageStream = new MemoryStream();
                        bitmap.Save(imageStream, ImageFormat.Jpeg);
                        stream = new MemoryStream(imageStream.ToArray());
                    }                    
                }
                
                await cblob.DeleteIfExistsAsync();
                await cblob.UploadFromStreamAsync(stream);
                
                return (true, WebUtility.UrlDecode(cblob.Uri.AbsoluteUri));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"BlobService - Error in UploadFileBlob");                
            }

            return (false, "");
        }

        private string GetHashMD5(string name)
        {
            var bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(name));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();

        }
      
        private async Task<CloudBlobContainer> CreateBlobContainerIfNotExistsAsync() 
        {
            CloudBlobContainer cloudBlobContainer = _client.GetContainerReference(BlobContainerName);

            await cloudBlobContainer.CreateIfNotExistsAsync();

            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await cloudBlobContainer.SetPermissionsAsync(permissions);

            return cloudBlobContainer;
        }
    }
}
