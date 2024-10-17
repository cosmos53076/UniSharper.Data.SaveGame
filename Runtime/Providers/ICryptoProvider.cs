// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharper.Data.SaveGame.Providers
{
    /// <summary>
    /// Provides the methods for encrypting and decrypting the save game data.
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="key">The key to be used for the encryption algorithm.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] data, byte[] key);

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="data">The data to be decrypted.</param>
        /// <param name="key">The key to be used for the decryption algorithm.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] data, byte[] key);
    }
}