using UnityEngine;
using Vldv.Step;
using System.Collections.Generic;

public class RunStepCode : MonoBehaviour {
    private string _death;
    private readonly Queue<string> _gossipQueue = new();

    // ReSharper disable MemberCanBePrivate.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global, UnassignedField.Global
    public int MaxGossipCount = 10;
    public int MaxStepAttempts = 5;
    // ReSharper restore UnassignedField.Global, ConvertToConstant.Global, FieldCanBeMadeReadOnly.Global, MemberCanBePrivate.Global

    public bool PauseOnDeath() {
        try { 
            _death = StepCode.Run("Death");
        } catch (Step.Interpreter.CallFailedException) {
            _death = "";
        }
        return _death != "";
    }

    public void ShowDeath() => GUI.Label(new Rect(600, 100, 1000, 1000), _death);

    public void ProcessGossip() {
        var tryCounter = 0;
        var newGossip = "";
        while (tryCounter <= MaxStepAttempts) {
            newGossip = StepCode.Run("Gossip");
            if (!_gossipQueue.Contains(newGossip)) {
                break;
            }
        }
        _gossipQueue.Enqueue(newGossip);
        if (_gossipQueue.Count > MaxGossipCount) _gossipQueue.Dequeue();
    }

    public void ShowGossip() => GUI.Label(new Rect(600, 200, 1000, 1000),
        string.Join("\n", _gossipQueue.ToArray()));
}
