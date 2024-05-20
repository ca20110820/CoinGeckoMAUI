using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.DataVisuals
{
    public static class VisualUtility
    {
        /// <summary>
        /// Asynchronously fetches an image source from the specified URL.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <returns>An <see cref="ImageSource"/> representing the image.</returns>
        public static async Task<ImageSource> GetImageSourceAsync(string url)
        {
            byte[] imageData = await FetchByteArrayFromURLAsync(url);

            // Convert byte array to stream
            Stream imageStream = new MemoryStream(imageData);

            // Create BitmapImageSource from stream
            return ImageSource.FromStream(() => new MemoryStream(imageData));
        }

        /// <summary>
        /// Asynchronously fetches a byte array from the specified URL.
        /// </summary>
        /// <param name="url">The URL of the resource.</param>
        /// <returns>A byte array representing the resource.</returns>
        public static async Task<byte[]> FetchByteArrayFromURLAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    throw new Exception($"Failed to fetch the byte array. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
