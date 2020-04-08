// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License. See LICENSE in the
// project root for license information.

namespace UniSharper.Data.SaveGame
{
    /// <summary>
    /// Provides the methods for encrypt/decrypt the data of SaveGameManager.
    /// </summary>
    public interface ISaveGameCryptoProvider
    {
        #region Properties

        /// <summary>
        /// Gets or sets the key to be used for the encryption algorithm.
        /// </summary>
        /// <value>The key to be used for the encryption algorithm.</value>
        byte[] Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the key string to be used for the encryption algorithm.
        /// </summary>
        /// <value>The key string to be used for the encryption algorithm.</value>
        string KeyString
        {
            get;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="data">The data to be decrypted.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] data);

        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] data);

        #endregion Methods
    }
}