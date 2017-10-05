using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DataLightViewer.Helpers
{
    public static class TextHelper
    {
        public static async Task WriteTextAsync(this string content, string pathToFile)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException($"{nameof(content)}");
            if (string.IsNullOrEmpty(pathToFile))
                throw new ArgumentException($"{nameof(pathToFile)}");

            byte[] encodedText = Encoding.Unicode.GetBytes(content);

            using (FileStream sourceStream = new FileStream(pathToFile,
                    FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }


        public static async Task WriteToFileAsync(this string content, string pathToFile)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException($"{nameof(content)}");
            if (string.IsNullOrEmpty(pathToFile))
                throw new ArgumentException($"{nameof(pathToFile)}");

               using (var writer = File.CreateText(pathToFile))
                   await writer.WriteAsync(content);
        }
    }
}
