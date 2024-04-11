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
    public class SaveGameManager : ISaveGameManager
    {
        private const int EncryptionKeyLength = 16;

        public static readonly Encoding DefaultEncoding = Encoding.UTF8;

        private static readonly string EditorDefaultStorePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(Directory.GetCurrentDirectory(), "Saves"));

        private static readonly string RuntimeDefaultStorePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(Application.persistentDataPath, "saves"));

        private Dictionary<string, FileStream> fileStreamMap;
        
        public string StorePath { get; private set; }
        
        public ISaveGameDataCryptoProvider SaveGameDataCryptoProvider { get; private set; }

        public virtual void Initialize(string storePath = null, ISaveGameDataCryptoProvider cryptoProvider = null)
        {
            fileStreamMap = new Dictionary<string, FileStream>();
            
            StorePath = !string.IsNullOrEmpty(storePath) ? storePath :
                PlayerEnvironment.IsEditorPlatform ? EditorDefaultStorePath : RuntimeDefaultStorePath;

            SaveGameDataCryptoProvider = cryptoProvider ?? new SaveGameDataCryptoProvider();
        }
        
        public virtual string GetFilePath(string name, bool autoCreateFolder = false)
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
        
        public virtual bool ExistsSaveData(string name)
        {
            var filePath = GetFilePath(name);
            return File.Exists(filePath);
        }
        
        public virtual bool TryLoadGame(string name, out string data)
        {
            var result = TryLoadGameData(name, out var rawData);
            data = result && rawData != null ? DefaultEncoding.GetString(rawData) : null;
            return result;
        }
        
        public virtual bool TryLoadGameData(string name, out byte[] data)
        {
            if (ExistsSaveData(name))
            {
                data = LoadGameData(name);
                return true;
            }

            data = null;
            return false;
        }
        
        public virtual string LoadGame(string name)
        {
            var gameData = LoadGameData(name);
            return gameData == null ? null : DefaultEncoding.GetString(gameData);
        }
        
        public virtual byte[] LoadGameData(string name)
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
            return SaveGameDataCryptoProvider?.Decrypt(cipherData, key);
        }
        
        public virtual void SaveGame(string name, string data, bool encrypt = true)
        {
            var rawData = DefaultEncoding.GetBytes(data);
            SaveGameData(name, rawData, encrypt);
        }
        
        public virtual void SaveGameData(string name, byte[] data, bool encrypt = true)
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
                    var cipherData = SaveGameDataCryptoProvider.Encrypt(data, key);
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
        
        public virtual void DeleteSaveData(string name)
        {
            if (!ExistsSaveData(name)) 
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
        
        public virtual void Dispose()
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