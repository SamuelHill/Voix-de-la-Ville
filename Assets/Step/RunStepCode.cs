using UnityEngine;
using Vldv.Step;
using System.Collections.Generic;

public class RunStepCode : MonoBehaviour {
    private string _death;
    private string _news;
    private string _marriage;
    private readonly Queue<string> _gossipQueue = new();

    // ReSharper disable MemberCanBePrivate.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global, UnassignedField.Global, InconsistentNaming
    public int MaxGossipCount = 10;
    public int MaxStepAttempts = 5;
    // ReSharper restore InconsistentNaming, UnassignedField.Global, ConvertToConstant.Global, FieldCanBeMadeReadOnly.Global, MemberCanBePrivate.Global

    private static bool TryRun(string stepString, out string stepOutput) {
        try {
            stepOutput = StepCode.Run(stepString);
        } catch (Step.Interpreter.CallFailedException) {
            stepOutput = "";
        }
        return stepOutput != "";
    }
    private static bool TryRunCheckIfNew(string stepString, out string stepOutput, string previousStepOutput) {
        try {
            stepOutput = StepCode.Run(stepString);
        } catch (Step.Interpreter.CallFailedException) {
            stepOutput = "";
        }
        if (stepOutput == previousStepOutput) {
            previousStepOutput = stepOutput;
        }
        return stepOutput != "";
    }

    public bool PauseOnDeath() => TryRun("Death", out _death);
    
    public bool PauseOnMarriage() => TryRun("Marriage", out _marriage);

    public void ProcessGossip() {
        var tryCounter = 0;
        var newGossip = "";
        while (tryCounter <= MaxStepAttempts) {
            newGossip = StepCode.Run("Gossip");
            if (!_gossipQueue.Contains(newGossip)) {
                break;
            }
            tryCounter++;
        }
        _gossipQueue.Enqueue(newGossip);
        if (_gossipQueue.Count > MaxGossipCount) _gossipQueue.Dequeue();
    }

    public void GetNews() => _news = StepCode.Run("Newspaper");
    
    public void ShowDeath() => GUI.Label(new Rect(600, 100, 1000, 1000), _death);

    public void ShowMarriage() => GUI.Label(new Rect(600, 150, 1000, 1000), _marriage);

    public void ShowGossip() => GUI.Label(new Rect(600, 200, 1000, 1000), string.Join("\n", _gossipQueue.ToArray()));

    public void ShowNews() => GUI.Label(new Rect(600, 400, 1000, 1000), _news);
}
