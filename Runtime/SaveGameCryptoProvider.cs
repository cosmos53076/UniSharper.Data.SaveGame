// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using ReSharp.Security.Cryptography;

namespace UniSharper.Data.SaveGame
{
    /// <summary>
    /// The default encryption provider for <see cref="UniSharper.Data.SaveGame.SaveGameManager"/>.
    /// Implements the <see cref="UniSharper.Data.SaveGame.ISaveGameCryptoProvider"/>
    /// </summary>
    /// <seealso cref="UniSharper.Data.SaveGame.ISaveGameCryptoProvider"/>
    public class SaveGameCryptoProvider : ISaveGameCryptoProvider
    {
        public byte[] Decrypt(byte[] data, byte[] key) => CryptoUtility.AesDecrypt(data, key);
        
        public byte[] Encrypt(byte[] data, byte[] key) => CryptoUtility.AesEncrypt(data, key);
    }
}