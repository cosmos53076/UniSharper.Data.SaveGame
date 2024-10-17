// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using ReSharp.Compression;

namespace UniSharper.Data.SaveGame.Providers
{
    /// <summary>
    /// Deflate algorithm implementation of <see cref="ICompressionProvider"/>.
    /// </summary>
    public class DeflateCompressionProvider : ICompressionProvider
    {
        public byte[] Compress(byte[] input) => Deflate.Compress(input);

        public byte[] Decompress(byte[] input) => Deflate.Decompress(input);
    }
}