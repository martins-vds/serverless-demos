using System.IO;
using ImageResizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace BlobStorageFunction
{
    public static class ImageResizerFunction
    {
        [FunctionName("ImageResizerFunction")]
        public static void Run([BlobTrigger("images/{name}", Connection = "BlobStorageAccountConfiguration")]Stream myBlob, [Blob("thumbnails/{name}", FileAccess.Write, Connection = "BlobStorageAccountConfiguration")] Stream thumbnail, string name, TraceWriter log)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var imageBuilder = ImageBuilder.Current;

            imageBuilder.Build(myBlob, thumbnail,
                new ResizeSettings(150, 150, FitMode.Max, null), false);
        }
    }
}
