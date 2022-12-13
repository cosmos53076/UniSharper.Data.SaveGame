// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReSharp.Extensions;
using ReSharp.Security.Cryptography;
using UnityEngine;
// ReSharper disable RedundantArgumentDefaultValue

namespace UniSharper.Data.SaveGame
{
    /// <summary>
    /// The SaveGameManager is a convenience class for managing the data of save game.
    /// </summary>
    public sealed class SaveGameManager : IDisposable
    {
        private const int EncryptionKeyLength = 16;

        public static readonly Encoding DefaultEncoding = Encoding.UTF8;

        private static readonly string EditorDefaultStorePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(Directory.GetCurrentDirectory(), "Saves"));

        private static readonly string RuntimeDefaultStorePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(Application.persistentDataPath, "saves"));

        private Dictionary<string, FileStream> fileStreamMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveGameManager"/> class.
        /// </summary>
        /// <param name="storePath">The store path where data to save.</param>
        /// <param name="cryptoProvider">The encryption provider.</param>
        public SaveGameManager(string storePath = null, ISaveGameCryptoProvider cryptoProvider = null)
        {
            fileStreamMap = new Dictionary<string, FileStream>();
            
            StorePath = !string.IsNullOrEmpty(storePath) ? storePath :
                PlayerEnvironment.IsEditorPlatform ? EditorDefaultStorePath : RuntimeDefaultStorePath;

            CryptoProvider = cryptoProvider ?? new SaveGameCryptoProvider();
        }

        /// <summary>
        /// Gets the encryption provider.
        /// </summary>
        /// <value>The encryption provider.</value>
        public ISaveGameCryptoProvider CryptoProvider { get; }

        /// <summary>
        /// Gets the store path where data to save.
        /// </summary>
        /// <value>The store path where data to save.</value>
        public string StorePath { get; }

        /// <summary>
        /// Deletes the save data.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        public void DeleteSaveData(string name)
        {
            if (!SaveDataExists(name)) 
                return;

            try
            {
                File.Delete(GetFilePath(name));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Can not delete save data, exception: {e}");
            }
        }

        /// <summary>
        /// Loads the game data of <see cref="System.String"/>.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <returns>The game data of <see cref="System.String"/> from file.</returns>
        public string LoadGame(string name)
        {
            var gameData = LoadGameData(name);
            return gameData == null ? null : DefaultEncoding.GetString(gameData);
        }

        /// <summary>
        /// Loads the raw data of game.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <returns>The raw data of game from file.</returns>
        public byte[] LoadGameData(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var filePath = GetFilePath(name);
            if (string.IsNullOrEmpty(filePath))
                return null;

            // Dispose file stream before loading game data.
            if (fileStreamMap.ContainsKey(name))
            {
                var fileStream = fileStreamMap[name];
                fileStream?.Dispose();
                fileStreamMap.Remove(name);
            }
                
            byte[] fileData;

            try
            {
                fileData = File.ReadAllBytes(filePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Can not load save game, exception: {e}");
                return null;
            }
            
            using var reader = new BinaryReader(new MemoryStream(fileData));
            var dataEncryptionFlagRawData = reader.ReadBytes(1);
            var dataEncryptionFlag = BitConverter.ToBoolean(dataEncryptionFlagRawData, 0);

            // No data encryption.
            if (!dataEncryptionFlag) 
                return reader.ReadBytes(fileData.Length - dataEncryptionFlagRawData.Length);
            
            // Need data decryption.
            var key = reader.ReadBytes(EncryptionKeyLength);
            var cipherData = reader.ReadBytes(fileData.Length - dataEncryptionFlagRawData.Length - EncryptionKeyLength);
            return CryptoProvider?.Decrypt(cipherData, key);
        }

        /// <summary>
        /// Determines whether the specified game data exists.
        /// </summary>
        /// <param name="name">The name of game data.</param>
        /// <returns>
        /// <c>true</c> if the caller has the required permissions and the game data exists,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool SaveDataExists(string name)
        {
            var filePath = GetFilePath(name);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Saves the game data of <see cref="System.String"/>.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">The save data of <see cref="System.String"/>.</param>
        /// <param name="encrypt">
        /// if set to <c>true</c> [encrypt the game data of <see cref="System.String"/> before saving].
        /// </param>
        public void SaveGame(string name, string data, bool encrypt = true)
        {
            var rawData = DefaultEncoding.GetBytes(data);
            SaveGameData(name, rawData, encrypt);
        }

        /// <summary>
        /// Saves the raw data of game.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">The raw save data of game.</param>
        /// <param name="encrypt">if set to <c>true</c> [encrypt the raw data of game before saving].</param>
        /// <exception cref="ArgumentNullException">name or rawData</exception>
        public void SaveGameData(string name, byte[] data, bool encrypt = true)
        {
            if (string.IsNullOrEmpty(name) || data == null)
                return;

            try
            {
                // Create new file stream.
                if (!fileStreamMap.TryGetValue(name, out var fileStream))
                {
                    var filePath = GetFilePath(name, true);
                    fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    fileStreamMap.Add(name, fileStream);
                }

                fileStream.Seek(0, SeekOrigin.Begin);
            
                var dataEncryptionFlag = BitConverter.GetBytes(encrypt);

                if (encrypt)
                {
                    var key = CryptoUtility.GenerateRandomKey(EncryptionKeyLength);
                    var cipherData = CryptoProvider.Encrypt(data, key);
                    fileStream.SetLength(dataEncryptionFlag.Length + key.Length + cipherData.Length);
                    fileStream.Write(dataEncryptionFlag, 0, dataEncryptionFlag.Length);
                    fileStream.Write(key, 0, key.Length);
                    fileStream.Write(cipherData, 0, cipherData.Length);
                }
                else
                {
                    fileStream.SetLength(dataEncryptionFlag.Length + data.Length);
                    fileStream.Write(dataEncryptionFlag, 0, dataEncryptionFlag.Length);
                    fileStream.Write(data, 0, data.Length);
                }
                
                fileStream.Flush(true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save game data failed, exception: {e}");
            }
        }

        /// <summary>
        /// Loads the game data of <see cref="System.String"/>. A return code indicates whether the
        /// operation succeeded or failed.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">
        /// When this method returns, contains the save data of <see cref="System.String"/>, if the
        /// operation succeeded, or an null value if the operation failed.
        /// <c>true</c> if the game data of <see cref="System.String"/> is loaded, <c>false</c> otherwise.
        /// </returns>
        public bool TryLoadGame(string name, out string data)
        {
            var result = TryLoadGameData(name, out var rawData);
            data = result && rawData != null ? DefaultEncoding.GetString(rawData) : null;
            return result;
        }

        /// <summary>
        /// Loads the raw data of game. A return code indicates whether the operation succeeded or failed.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">
        /// When this method returns, contains the raw save data of game, if the operation
        /// succeeded, or an null value if the operation failed.
        /// </param>
        /// <returns><c>true</c> if the raw data of game is loaded, <c>false</c> otherwise.</returns>
        public bool TryLoadGameData(string name, out byte[] data)
        {
            if (SaveDataExists(name))
            {
                data = LoadGameData(name);
                return true;
            }

            data = null;
            return false;
        }
        
        /// <summary>
        /// Get the file path to store with specified file name.
        /// </summary>
        /// <param name="name">The specified file name. </param>
        /// <param name="autoCreateFolder">Whether create folder if the folder under path do not exists. </param>
        /// <returns></returns>
        public string GetFilePath(string name, bool autoCreateFolder = false)
        {
            try
            {
                if (autoCreateFolder && !Directory.Exists(StorePath))
                {
                    Directory.CreateDirectory(StorePath);
                }
                
                return PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(StorePath, $"{name}.sav"));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Create save game folder failed, exception: {e}");
                return null;
            }
        }
        
        public void Dispose()
        {
            if (fileStreamMap == null)
                return;

            foreach (var pair in fileStreamMap)
            {
                pair.Value?.Dispose();
            }

            fileStreamMap = null;
        }
    }
}