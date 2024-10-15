using UnityEngine;
using UnityEngine.UI;

namespace UniSharper.Data.SaveGame.Samples
{
    public class SaveGameSample : MonoBehaviour
    {
        private const string GameDataName = "SaveGameExample";

        private const string Key = "SAnzX2EoRV9Haaqc";

        [SerializeField]
        private InputField inputFiled;

        [SerializeField]
        private Button loadGameButton;

        private ISaveGameManager manager;

        public void OnLoadGameButtonClicked()
        {
            LoadGame();
        }

        public void OnSaveGameButtonClicked()
        {
            if (!string.IsNullOrEmpty(inputFiled.text))
            {
                var success = manager.SaveGame(GameDataName, inputFiled.text, true, true);
                inputFiled.text = string.Empty;
                loadGameButton.interactable = success;
            }
        }

        private void Awake()
        {
            manager = new SaveGameManager();
            manager.Initialize();
            loadGameButton.interactable = manager.ExistsSaveData(GameDataName);
        }

        private void LoadGame()
        {
            var content = manager.LoadGame(GameDataName);
            if (string.IsNullOrEmpty(content))
                return;

            inputFiled.text = content;
        }
    }
}