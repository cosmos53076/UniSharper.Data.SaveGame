// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharper.Data.SaveGame
{
    /// <summary>
    /// Provides the methods for managing save game data.
    /// </summary>
    public interface ISaveGameManager : IDisposable
    {
        /// <summary>
        /// Gets the store path where data to save.
        /// </summary>
        /// <value>The store path where data to save.</value>
        string StorePath { get; }
        
        /// <summary>
        /// Gets the encryption provider of save game data.
        /// </summary>
        /// <value>The encryption provider.</value>
        ISaveGameDataCryptoProvider SaveGameDataCryptoProvider { get; }
        
        /// <summary>
        /// Initializes save game manager.
        /// </summary>
        /// <param name="storePath"></param>
        /// <param name="saveGameDataCryptoProvider"></param>
        void Initialize(string storePath = null, ISaveGameDataCryptoProvider saveGameDataCryptoProvider = null);

        /// <summary>
        /// Get the file path to store with specified file name.
        /// </summary>
        /// <param name="name">The specified file name. </param>
        /// <param name="autoCreateFolder">Whether create folder if the folder under path do not exists. </param>
        /// <returns></returns>
        string GetFilePath(string name, bool autoCreateFolder = false);

        /// <summary>
        /// Determines whether the specified game data exists.
        /// </summary>
        /// <param name="name">The name of game data.</param>
        /// <returns>
        /// <c>true</c> if the caller has the required permissions and the game data exists,
        /// <c>false</c> otherwise.
        /// </returns>
        bool ExistsSaveData(string name);

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
        bool TryLoadGame(string name, out string data);

        /// <summary>
        /// Loads the raw data of game. A return code indicates whether the operation succeeded or failed.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">
        /// When this method returns, contains the raw save data of game, if the operation
        /// succeeded, or an null value if the operation failed.
        /// </param>
        /// <returns><c>true</c> if the raw data of game is loaded, <c>false</c> otherwise.</returns>
        bool TryLoadGameData(string name, out byte[] data);

        /// <summary>
        /// Loads the game data of <see cref="System.String"/>.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <returns>The game data of <see cref="System.String"/> from file.</returns>
        string LoadGame(string name);

        /// <summary>
        /// Loads the raw data of game.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <returns>The raw data of game from file.</returns>
        byte[] LoadGameData(string name);

        /// <summary>
        /// Saves the game data of <see cref="System.String"/>.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">The save data of <see cref="System.String"/>.</param>
        /// <param name="encrypt">
        /// if set to <c>true</c> [encrypt the game data of <see cref="System.String"/> before saving].
        /// </param>
        void SaveGame(string name, string data, bool encrypt = true);

        /// <summary>
        /// Saves the raw data of game.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">The raw save data of game.</param>
        /// <param name="encrypt">if set to <c>true</c> [encrypt the raw data of game before saving].</param>
        /// <exception cref="ArgumentNullException">name or rawData</exception>
        void SaveGameData(string name, byte[] data, bool encrypt = true);

        /// <summary>
        /// Deletes the save data.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        void DeleteSaveData(string name);
    }
}