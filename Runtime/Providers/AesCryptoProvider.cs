// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using ReSharp.Security.Cryptography;

namespace UniSharper.Data.SaveGame.Providers
{
    /// <summary>
    /// AES algorithm implementation of <see cref="ICryptoProvider"/>.
    /// </summary>
    public class AesCryptoProvider : ICryptoProvider
    {
        public byte[] Encrypt(byte[] data, byte[] key) => CryptoUtility.AesEncrypt(data, key);

        public byte[] Decrypt(byte[] data, byte[] key) => CryptoUtility.AesDecrypt(data, key);
    }
}