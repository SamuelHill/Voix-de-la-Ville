using UnityEngine;
using Vldv.Step;
using System.Collections.Generic;

public class RunStepCode : MonoBehaviour {
    private string _death;
    private string _news;
    private string _marriage;
    private Queue<string> _gossipQueue = new();

    // ReSharper disable MemberCanBePrivate.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global, UnassignedField.Global
    public int MaxGossipCount = 10;
    public int MaxStepAttempts = 5;
    // ReSharper restore UnassignedField.Global, ConvertToConstant.Global, FieldCanBeMadeReadOnly.Global, MemberCanBePrivate.Global

    private static bool TryRun(string stepString, out string stepOutput) {
        try {
            stepOutput = StepCode.Run(stepString);
        } catch (Step.Interpreter.CallFailedException) {
            stepOutput = "";
        }
        return stepOutput != "";
    }

    public bool PauseOnDeath() => TryRun("Death", out _death);

    public void ShowDeath() => GUI.Label(new Rect(600, 100, 1000, 1000), _death);

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

    public void ShowGossip() => GUI.Label(new Rect(600, 200, 1000, 1000),
        string.Join("\n", _gossipQueue.ToArray()));

    public void getNews() {
        _news = StepCode.Run("Newspaper");
    }

    public void ShowNews() => GUI.Label(new Rect(600, 500, 1000, 1000), _news);

    public bool PauseOnMarriage() {
        try {
            _marriage = StepCode.Run("Marriage");
        }
        catch (Step.Interpreter.CallFailedException e) {
            _marriage = "";
        }
        return _marriage != "";
    }

    public void ShowMarriage() => GUI.Label(new Rect(600, 700, 1000, 1000), _marriage);
}
