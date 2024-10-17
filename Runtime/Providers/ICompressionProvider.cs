// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharper.Data.SaveGame.Providers
{
    /// <summary>
    /// Provides the methods for save game data compression and decompression.
    /// </summary>
    public interface ICompressionProvider
    {
        /// <summary>
        /// Compresses the input data.
        /// </summary>
        /// <param name="input">The uncompressed data. </param>
        /// <returns>The compressed data. </returns>
        byte[] Compress(byte[] input);

        /// <summary>
        /// Decompresses the input data.
        /// </summary>
        /// <param name="input">The compressed data. </param>
        /// <returns>The uncompressed data. </returns>
        byte[] Decompress(byte[] input);
    }
}