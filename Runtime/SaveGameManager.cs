// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace UniSharper.Data.SaveGame
{
    /// <summary>
    /// The SaveGameManager is a convenience class for managing the data of save game.
    /// </summary>
    public sealed class SaveGameManager
    {
        #region Fields

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        private static readonly string EditorDefaultStorePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(Directory.GetCurrentDirectory(), "Saves"));

        private static readonly string RuntimeDefaultStorePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(Application.persistentDataPath, "saves"));

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveGameManager"/> class.
        /// </summary>
        /// <param name="storePath">The store path where data to save.</param>
        /// <param name="cryptoProvider">The encryption provider.</param>
        public SaveGameManager(string storePath = null, ISaveGameCryptoProvider cryptoProvider = null)
        {
            StorePath = !string.IsNullOrEmpty(storePath) ? storePath :
                PlayerEnvironment.IsEditorPlatform ? EditorDefaultStorePath : RuntimeDefaultStorePath;

            CryptoProvider = cryptoProvider ?? new SaveGameCryptoProvider();
        }

        #endregion Constructors

        #region Properties

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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Deletes the save data.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        public void DeleteSaveData(string name)
        {
            if (SaveDataExists(name))
            {
                File.Delete(GetFilePath(name));
            }
        }

        /// <summary>
        /// Loads the game data of <see cref="System.String"/>.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="decrypt">if set to <c>true</c> [decrypts the file data].</param>
        /// <returns>The game data of <see cref="System.String"/> from file.</returns>
        public string LoadGame(string name, bool decrypt = true) => DefaultEncoding.GetString(LoadGameData(name, decrypt));

        /// <summary>
        /// Loads the raw data of game.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="decrypt">if set to <c>true</c> [decrypts the file data].</param>
        /// <returns>The raw data of game from file.</returns>
        public byte[] LoadGameData(string name, bool decrypt = true)
        {
            var filePath = GetFilePath(name);
            var fileData = File.ReadAllBytes(filePath);
            var data = fileData;

            if (decrypt && CryptoProvider.Key != null)
            {
                data = CryptoProvider.Decrypt(fileData);
            }

            return data ?? fileData;
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
            string filePath = GetFilePath(name);
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
            byte[] rawData = DefaultEncoding.GetBytes(data);
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
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var fileData = data;

            if (encrypt && CryptoProvider.Key != null)
            {
                fileData = CryptoProvider.Encrypt(data);
            }

            var filePath = GetFilePath(name, true);
            File.WriteAllBytes(filePath, fileData);
        }

        /// <summary>
        /// Loads the game data of <see cref="System.String"/>. A return code indicates whether the
        /// operation succeeded or failed.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">
        /// When this method returns, contains the save data of <see cref="System.String"/>, if the
        /// operation succeeded, or an null value if the operation failed.
        /// </param>
        /// <param name="decrypt">if set to <c>true</c> [decrypts the file data].</param>
        /// <returns>
        /// <c>true</c> if the game data of <see cref="System.String"/> is loaded, <c>false</c> otherwise.
        /// </returns>
        public bool TryLoadGame(string name, out string data, bool decrypt = true)
        {
            var result = TryLoadGameData(name, out byte[] rawData, decrypt);

            if (result && rawData != null)
            {
                data = DefaultEncoding.GetString(rawData);
            }
            else
            {
                data = null;
            }

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
        /// <param name="decrypt">if set to <c>true</c> [decrypts the file data].</param>
        /// <returns><c>true</c> if the raw data of game is loaded, <c>false</c> otherwise.</returns>
        public bool TryLoadGameData(string name, out byte[] data, bool decrypt = true)
        {
            if (SaveDataExists(name))
            {
                data = LoadGameData(name, decrypt);
                return true;
            }

            data = null;
            return false;
        }

        private string GetFilePath(string name, bool autoCreateFolder = false)
        {
            if (autoCreateFolder && !Directory.Exists(StorePath))
            {
                Directory.CreateDirectory(StorePath);
            }

            return PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(StorePath, $"{name}.sav"));
        }

        #endregion Methods
    }
}