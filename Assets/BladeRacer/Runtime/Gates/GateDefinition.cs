using UnityEngine;

namespace BladeRacer.Gates
{
    public enum GateKind { Checkpoint, LapLine, Boost, Hazard, RouteSwitch }

    [CreateAssetMenu(menuName = "Blade Racer/Gate Definition", fileName = "GateDefinition")]
    public sealed class GateDefinition : ScriptableObject
    {
        [SerializeField] private string gateId = "gate";
        [SerializeField] private GateKind kind;
        [SerializeField] private int sequenceIndex;
        [SerializeField] private float value = 1f;
        [SerializeField] private bool oneShot;
        public string GateId => gateId;
        public GateKind Kind => kind;
        public int SequenceIndex => sequenceIndex;
        public float Value => value;
        public bool OneShot => oneShot;
    }
}
