using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UniSharper.Data.SaveGame.Samples
{
    public class SaveGameSample : MonoBehaviour
    {
        #region Fields

        private const string GameDataName = "SaveGameExample";

        private const string Key = "SAnzX2EoRV9Haaqc";

        [SerializeField]
        private InputField inputFiled = null;

        [SerializeField]
        private Button loadGameButton = null;

        private SaveGameManager manager = null;

        #endregion Fields

        #region Methods

        public void OnLoadGameButtonClicked()
        {
            LoadGame();
        }

        public void OnSaveGameButtonClicked()
        {
            if (!string.IsNullOrEmpty(inputFiled.text))
            {
                manager.SaveGame(GameDataName, inputFiled.text);
            }
        }

        private void Awake()
        {
            manager = new SaveGameManager(null, new SaveGameCryptoProvider(Encoding.UTF8.GetBytes(Key)));
            loadGameButton.interactable = manager.SaveDataExists(GameDataName);
            LoadGame();
        }

        private void LoadGame()
        {
            var content = manager.LoadGame(GameDataName);
            if (string.IsNullOrEmpty(content))
                return;

            inputFiled.text = content;
        }

        #endregion Methods
    }
}