using System.Collections.Generic;
using Step.Interpreter;
using UnityEngine;

namespace VdlV.Step {
    using static StepCode;
    using static GUI;
    
    public class RunStepCode : MonoBehaviour {
        private readonly Queue<string> _gossipQueue = new();
        private string _death;
        private string _marriage;
        private string _news;

        // ReSharper disable MemberCanBePrivate.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global, UnassignedField.Global, InconsistentNaming
        public int MaxGossipCount = 10;
        public int MaxStepAttempts = 5;
        // ReSharper restore InconsistentNaming, UnassignedField.Global, ConvertToConstant.Global, FieldCanBeMadeReadOnly.Global, MemberCanBePrivate.Global

        private static bool TryRun(string stepString, out string stepOutput) {
            try { stepOutput = Run(stepString); } catch (CallFailedException) { stepOutput = ""; }
            return stepOutput != "";
        }

        public bool PauseOnDeath() => TryRun("Death", out _death);

        public bool PauseOnMarriage() => TryRun("Marriage", out _marriage);

        public void ProcessGossip() {
            var tryCounter = 0;
            var newGossip = "";
            while (tryCounter <= MaxStepAttempts) {
                newGossip = Run("Gossip");
                if (!_gossipQueue.Contains(newGossip)) break;
                tryCounter++;
            }
            _gossipQueue.Enqueue(newGossip);
            if (_gossipQueue.Count > MaxGossipCount) _gossipQueue.Dequeue();
        }

        public void GetNews() => _news = Run("Newspaper");

        public void ShowDeath() => Label(new Rect(600, 100, 1000, 1000), _death);

        public void ShowMarriage() => Label(new Rect(600, 150, 1000, 1000), _marriage);

        public void ShowGossip() => Label(new Rect(600, 200, 1000, 1000), string.Join("\n", _gossipQueue.ToArray()));

        public void ShowNews() => Label(new Rect(600, 400, 1000, 1000), _news);
    }
}
