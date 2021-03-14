// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using ReSharp.Security.Cryptography;
using System.Text;

namespace UniSharper.Data.SaveGame
{
    /// <summary>
    /// The default encryption provider for <see cref="UniSharper.Data.SaveGame.SaveGameManager"/>.
    /// Implements the <see cref="UniSharper.Data.SaveGame.ISaveGameCryptoProvider"/>
    /// </summary>
    /// <seealso cref="UniSharper.Data.SaveGame.ISaveGameCryptoProvider"/>
    public class SaveGameCryptoProvider : ISaveGameCryptoProvider
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveGameCryptoProvider"/> class.
        /// </summary>
        /// <param name="key">The key for encryption.</param>
        public SaveGameCryptoProvider(byte[] key = null) => Key = key != null && key.Length > 0 ? key : CryptoUtility.GenerateRandomKey(32, true, true, true, false);

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the key to be used for the encryption algorithm.
        /// </summary>
        /// <value>The key to be used for the encryption algorithm.</value>
        public byte[] Key { get; set; }

        /// <summary>
        /// Gets the key string to be used for the encryption algorithm.
        /// </summary>
        /// <value>The key string to be used for the encryption algorithm.</value>
        public string KeyString => Encoding.UTF8.GetString(Key);

        #endregion Properties

        #region Methods

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="data">The data to be decrypted.</param>
        /// <returns>The decrypted data.</returns>
        public byte[] Decrypt(byte[] data) => CryptoUtility.AesDecrypt(data, Key);

        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>The encrypted data.</returns>
        public byte[] Encrypt(byte[] data) => CryptoUtility.AesEncrypt(data, Key);

        #endregion Methods
    }
}