using System;
using System.Collections.Generic;
using BladeRacer.Gates;
using UnityEngine;

namespace BladeRacer.Race
{
    public sealed class RaceDirector : MonoBehaviour
    {
        [SerializeField, Min(1)] private int lapsToWin = 3;
        [SerializeField, Min(0)] private int checkpointCount;
        private readonly Dictionary<GameObject, RacerProgress> racers = new();
        public event Action<GameObject, int> LapCompleted;
        public event Action<GameObject> RaceCompleted;

        private void OnEnable() => TrackGate.Triggered += HandleGate;
        private void OnDisable() => TrackGate.Triggered -= HandleGate;

        private void HandleGate(TrackGate gate, GameObject racer)
        {
            if (!racers.TryGetValue(racer, out var progress)) progress = new RacerProgress();
            var definition = gate.Definition;
            if (definition.Kind == GateKind.Checkpoint && definition.SequenceIndex == progress.nextCheckpoint)
                progress.nextCheckpoint++;
            else if (definition.Kind == GateKind.LapLine && progress.nextCheckpoint >= checkpointCount)
            {
                progress.lap++; progress.nextCheckpoint = 0;
                LapCompleted?.Invoke(racer, progress.lap);
                if (progress.lap >= lapsToWin) RaceCompleted?.Invoke(racer);
            }
            racers[racer] = progress;
        }

        private struct RacerProgress { public int lap; public int nextCheckpoint; }
    }
}
