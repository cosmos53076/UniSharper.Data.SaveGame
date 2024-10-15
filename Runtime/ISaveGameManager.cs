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
        /// Gets the crypto provider.
        /// </summary>
        /// <value>The encryption provider.</value>
        ICryptoProvider CryptoProvider { get; }

        /// <summary>
        /// Gets the compression provider.
        /// </summary>
        ICompressionProvider CompressionProvider { get; }

        /// <summary>
        /// Initializes save game manager.
        /// </summary>
        /// <param name="storePath">The store path where data to save.</param>
        /// <param name="cryptoProvider">The crypto provider.</param>
        /// <param name="compressionProvider">The compression provider.</param>
        void Initialize(string storePath = null, ICryptoProvider cryptoProvider = null, ICompressionProvider compressionProvider = null);

        /// <summary>
        /// Get the file path to store with specified file name.
        /// </summary>
        /// <param name="name">The specified file name. </param>
        /// <param name="autoCreateFolder">Whether create folder if the folder under path do not exist. </param>
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
        /// operation succeeded, or a null value if the operation failed.
        /// <c>true</c> if the game data of <see cref="System.String"/> is loaded, <c>false</c> otherwise.
        /// </returns>
        bool TryLoadGame(string name, out string data);

        /// <summary>
        /// Loads the raw data of game. A return code indicates whether the operation succeeded or failed.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">
        /// When this method returns, contains the raw save data of game, if the operation
        /// succeeded, or a null value if the operation failed.
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
        /// <param name="encrypt">if set to <c>true</c> [encrypt the game data of <see cref="System.String"/> before saving].</param>
        /// <param name="compress">if set to <c>true</c> [compress the game data of <see cref="System.String"/> before saving].</param>
        /// <returns><c>true</c> if data successfully saved, <c>false</c> otherwise.</returns>
        bool SaveGame(string name,
            string data,
            bool encrypt = true,
            bool compress = false);

        /// <summary>
        /// Saves the raw data of game.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        /// <param name="data">The raw save data of game.</param>
        /// <param name="encrypt">if set to <c>true</c> [encrypt the raw data of game before saving].</param>
        /// <param name="compress">if set to <c>true</c> [compress the raw data of game before saving].</param>
        /// <returns><c>true</c> if data successfully saved, <c>false</c> otherwise.</returns>
        bool SaveGameData(string name,
            byte[] data,
            bool encrypt = true,
            bool compress = false);

        /// <summary>
        /// Deletes the save data.
        /// </summary>
        /// <param name="name">The name of save data.</param>
        void DeleteSaveData(string name);
    }
}