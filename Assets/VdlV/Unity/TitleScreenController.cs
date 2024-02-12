using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VdlV.Utilities;

namespace VdlV.Unity {
    using static SaveManager;
    using static SceneManager;

    public class TitleScreenController : MonoBehaviour {
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnassignedField.Global
        public TMP_Dropdown LoadFromSave;

        private void Start() {
            foreach (var save in SavesList()) 
                LoadFromSave.options.Add(new TMP_Dropdown.OptionData(save));
        }

        private void Update() {}

        public void StartVdlV() {
            UpdateSaveToLoad(LoadFromSave);
            LoadScene("Voix de la Ville");
        }
    }
}
