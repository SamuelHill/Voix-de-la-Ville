using UnityEngine;
using Vldv.Step;

public class RunStepCode : MonoBehaviour
{
    public void OnGUI()
    {
        GUI.Label(new Rect(600, 100, 1000, 1000), StepCode.Run("DoSomething"));
    }
}
