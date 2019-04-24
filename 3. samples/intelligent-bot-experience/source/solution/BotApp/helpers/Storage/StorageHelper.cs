using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace BotApp
{
    public class StorageHelper
    {
        private static CloudStorageAccount GetCloudStorageAccount(string connectionString)
        {
            return CloudStorageAccount.Parse(connectionString);
        }

        private static CloudBlobClient GetCloudBlobClient(CloudStorageAccount account)
        {
            return account.CreateCloudBlobClient();
        }

        private static async Task<CloudBlobContainer> GetCloudBlobContainer(CloudBlobClient client, string container)
        {
            var c = client.GetContainerReference(container);
            await c.CreateIfNotExistsAsync();
            await c.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            return client.GetContainerReference(container);
        }

        public static async Task<string> UploadFileAsync(Stream stream, string filename,
                                                           string container, string connectionString,
                                                           string contentType)
        {
            CloudStorageAccount cloudStorageAccount = GetCloudStorageAccount(connectionString);
            CloudBlobClient blobClient = GetCloudBlobClient(cloudStorageAccount);
            CloudBlobContainer blobContainer = await GetCloudBlobContainer(blobClient, container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);

            filename = filename.Replace("\"", "");
            stream.Seek(0, SeekOrigin.Begin);

            blockBlob = blobContainer.GetBlockBlobReference(filename);

            blockBlob.Properties.ContentType = contentType;
            await blockBlob.UploadFromStreamAsync(stream);

            return blockBlob.Uri.ToString();
        }

        public static async Task<bool> DeleteFileAsync(string filename, string container, string connectionString)
        {
            CloudStorageAccount cloudStorageAccount = GetCloudStorageAccount(connectionString);
            CloudBlobClient blobClient = GetCloudBlobClient(cloudStorageAccount);
            CloudBlobContainer blobContainer = await GetCloudBlobContainer(blobClient, container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);

            return await blockBlob.DeleteIfExistsAsync();
        }
    }
}