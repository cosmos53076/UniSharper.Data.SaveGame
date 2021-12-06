// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharper.Data.SaveGame
{
    /// <summary>
    /// Provides the methods for encrypt/decrypt the data of SaveGameManager.
    /// </summary>
    public interface ISaveGameCryptoProvider
    {
        #region Methods

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="data">The data to be decrypted.</param>
        /// <param name="key">The key to be used for the decryption algorithm.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] data, byte[] key);

        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="key">The key to be used for the encryption algorithm.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] data, byte[] key);

        #endregion Methods
    }
}