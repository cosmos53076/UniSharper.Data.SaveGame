// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReSharp.Extensions;
using ReSharp.Security.Cryptography;
using UniSharper.Data.SaveGame.Providers;
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

        public ICryptoProvider CryptoProvider { get; private set; }

        public ICompressionProvider CompressionProvider { get; private set; }

        public virtual void Initialize(string storePath = null, ICryptoProvider cryptoProvider = null, ICompressionProvider compressionProvider = null)
        {
            fileStreamMap = new Dictionary<string, FileStream>();
            StorePath = !string.IsNullOrEmpty(storePath) ? storePath : PlayerEnvironment.IsEditorPlatform ? EditorDefaultStorePath : RuntimeDefaultStorePath;
            CryptoProvider = cryptoProvider ?? new AesCryptoProvider();
            CompressionProvider = compressionProvider ?? new DeflateCompressionProvider();
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

            try
            {
                using (var reader = new BinaryReader(new MemoryStream(fileData)))
                {
                    var encryptionFlagRawData = reader.ReadBytes(1);
                    var encryptionFlag = BitConverter.ToBoolean(encryptionFlagRawData, 0);

                    if (encryptionFlag)
                    {
                        // Need to decrypt data.
                        var key = reader.ReadBytes(EncryptionKeyLength);
                        var compressionFlagRawData = reader.ReadBytes(1);
                        var compressionFlag = BitConverter.ToBoolean(compressionFlagRawData, 0);
                        var cipherData = reader.ReadBytes(fileData.Length - encryptionFlagRawData.Length - EncryptionKeyLength - compressionFlagRawData.Length);
                        var data = CryptoProvider.Decrypt(cipherData, key);
                        return compressionFlag ? CompressionProvider.Decompress(data) : data;
                    }
                    else
                    {
                        // No need to decrypt data.
                        var compressionFlagRawData = reader.ReadBytes(1);
                        var compressionFlag = BitConverter.ToBoolean(compressionFlagRawData, 0);
                        var data = reader.ReadBytes(fileData.Length - encryptionFlagRawData.Length - compressionFlagRawData.Length);
                        return compressionFlag ? CompressionProvider.Decompress(data) : data;
                    }
                }
            }
            catch (Exception)
            {
                // Try to load data from old version.
                return LoadOldVersionGameData(fileData);
            }
        }

        public virtual bool SaveGame(string name,
            string data,
            bool encrypt = true,
            bool compress = false)
        {
            var rawData = DefaultEncoding.GetBytes(data);
            return SaveGameData(name, rawData, encrypt, compress);
        }

        public virtual bool SaveGameData(string name,
            byte[] data,
            bool encrypt = true,
            bool compress = false)
        {
            if (string.IsNullOrEmpty(name) || data == null)
                return false;

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
                
                var encryptionFlag = BitConverter.GetBytes(encrypt);
                var compressionFlag = BitConverter.GetBytes(compress);

                if (encrypt)
                {
                    var key = CryptoUtility.GenerateRandomKey(EncryptionKeyLength);
                    var output = compress ? CompressionProvider.Compress(data) : data;
                    var cipherData = CryptoProvider.Encrypt(output, key);
                    fileStream.SetLength(encryptionFlag.Length + key.Length + compressionFlag.Length + cipherData.Length);
                    fileStream.Write(encryptionFlag, 0, encryptionFlag.Length);
                    fileStream.Write(key, 0, key.Length);
                    fileStream.Write(compressionFlag, 0, compressionFlag.Length);
                    fileStream.Write(cipherData, 0, cipherData.Length);
                }
                else
                {
                    var output = compress ? CompressionProvider.Compress(data) : data;
                    fileStream.SetLength(encryptionFlag.Length + compressionFlag.Length + output.Length);
                    fileStream.Write(encryptionFlag, 0, encryptionFlag.Length);
                    fileStream.Write(compressionFlag, 0, compressionFlag.Length);
                    fileStream.Write(output, 0, output.Length);
                }

                fileStream.Flush(true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save game data failed, exception: {e}");
                return false;
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
        
        private byte[] LoadOldVersionGameData(byte[] fileData)
        {
            try
            {
                using (var reader = new BinaryReader(new MemoryStream(fileData)))
                {
                    var encryptionFlagRawData = reader.ReadBytes(1);
                    var encryptionFlag = BitConverter.ToBoolean(encryptionFlagRawData, 0);
                    if (!encryptionFlag) 
                        return reader.ReadBytes(fileData.Length - encryptionFlagRawData.Length);
                    
                    // Need to decrypt data.
                    var key = reader.ReadBytes(EncryptionKeyLength);
                    var cipherData = reader.ReadBytes(fileData.Length - encryptionFlagRawData.Length - EncryptionKeyLength);
                    return CryptoProvider.Decrypt(cipherData, key);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
                return null;
            }
        }
    }
}