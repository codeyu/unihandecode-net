using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Unihandecode
{
    public class Unidecoder
    {
        public Unidecoder()
        {

        }

        private void LoadCodePoints(string lang)
        {
            var assembly = typeof(Unihandecode.Unidecoder).GetTypeInfo().Assembly;
            using(var resource = assembly.GetManifestResourceStream($"Unihandecode._gz.{lang}_codepoints.gz"))
            using (GZipStream decompressionStream = new GZipStream(resource, CompressionMode.Decompress))
            {
                using (StreamReader unzip = new StreamReader(decompressionStream))
                {
                    while (!unzip.EndOfStream)
                    {
                        Console.WriteLine(unzip.ReadLine());
                    }
                        
                }

            }

        }
    }
}