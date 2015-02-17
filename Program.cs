using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
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
            //RunTest(BlobCache.LocalMachine).Wait();
            RunTest2(BlobCache.LocalMachine).Wait();
            Console.WriteLine("Test Complete. Hit a key to quit.");
            Console.ReadKey();
        }

        static async Task RunTest(IBlobCache cache)
        {
            await cache.InvalidateAll();

            var url = "http://lacuadramagazine.com/wp-content/uploads/sangeh-monkey-forest-101.jpg";

            // on first pass, we know the image is not cached
            var image1 = await GetImage(cache, url);
            Debug.Assert(image1 == null);

            // second time, we must get it from the cache
            var image2 = await GetImage(cache, url);
            Debug.Assert(image2 != null);
        }

        static async Task RunTest2(IBlobCache cache)
        {
            await cache.InvalidateAll();

            var url = "http://lacuadramagazine.com/wp-content/uploads/sangeh-monkey-forest-101.jpg";
            var key = "demo2-" + url;
            // on first pass, we'll hit the network
            var image1 = await cache.LoadImageFromUrl(key, url);
            Debug.Assert(image1 != null);

            // second time, we must get it from the cache
            var image2 = await cache.LoadImageFromUrl(key, url);
            Debug.Assert(image2 != null);
        }

        static async Task<IBitmap> GetImage(IBlobCache cache, string url)
        {
            IBitmap image = null;
            try
            {
                var bytes = await cache.Get(url);
                image = await BitmapLoader.Current.Load(new MemoryStream(bytes), null, null);
                Console.WriteLine("Image returned from cache ({0}, {1})", image.Width, image.Height);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Image not found in the cache, making network request");
            }

            if (image == null)
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);
                var newBytes = await response.Content.ReadAsByteArrayAsync();
                await cache.Insert(url, newBytes);
            }

            return image;
        }
    }
}
