// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.IO;
using System.IO.Compression;

namespace UniSharper.Data.SaveGame
{
    internal class DefaultCompressionProvider : ICompressionProvider
    {
        public byte[] Compress(byte[] input)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(outputStream, CompressionLevel.Fastest))
                {
                    deflateStream.Write(input, 0, input.Length);
                    deflateStream.Flush();
                }

                return outputStream.ToArray();
            }
        }

        public byte[] Decompress(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            {
                using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        deflateStream.CopyTo(outputStream);
                        return outputStream.ToArray();
                    }
                }
            }
        }
    }
}