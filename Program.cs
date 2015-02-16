using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Akavache;
using Splat;

namespace SplatImageProblemExample
{
    class Program
    {
        static void Main(string[] args)
        {
            BlobCache.ApplicationName = "SplatImageProblemExample";
            RunTest().Wait();
            Console.WriteLine("Test Complete. Hit a key to quit.");
            Console.ReadKey();
        }

        static async Task RunTest()
        {
            Console.WriteLine("Starting caching test...");

            Console.WriteLine("Requesting the image for the first time");
            var url = "http://lacuadramagazine.com/wp-content/uploads/sangeh-monkey-forest-101.jpg";
            var image1 = await GetImage(url);
            Console.WriteLine(string.Format("Image received was ({0}, {1})", image1.Width, image1.Height));

            Console.WriteLine("Requesting the image for the second time");
            var image2 = await GetImage(url);
            await GetImage(url);
            Console.WriteLine(string.Format("Image received was ({0}, {1})", image2.Width, image2.Height));
        }

        static async Task<IBitmap> GetImage2(string url)
        {
            return await BlobCache.LocalMachine.LoadImageFromUrl(url);
        }

        static async Task<IBitmap> GetImage(string url)
        {
            IBitmap image;
            try
            {
                // get the image from the cache if it exists...
                image = await BlobCache.LocalMachine.GetObject<IBitmap>(url);
                Console.WriteLine("Image being returned from the cache");
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Image not found in the cache, making network request");
                image = null;
            }
            if (image != null)
                return image;

            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var stream = await response.Content.ReadAsStreamAsync();
            image = await BitmapLoader.Current.Load(stream, null, null);

            await BlobCache.LocalMachine.InsertObject(url, image, null);

            return image;
        }
    }
}
