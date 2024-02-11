using UnityEngine;
using UnityEngine.SceneManagement;

namespace VdlV.Unity {
    using static SceneManager;

    public class TitleScreenController : MonoBehaviour {
        private void Start() {}

        private void Update() {}

        public void StartVdlV() => LoadScene("Voix de la Ville");
    }
}
