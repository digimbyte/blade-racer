using System;
using UnityEngine;

namespace BladeRacer.Gates
{
    [RequireComponent(typeof(Collider))]
    public sealed class TrackGate : MonoBehaviour
    {
        [SerializeField] private GateDefinition definition;
        [SerializeField] private LayerMask vehicleLayers = ~0;
        private bool consumed;
        public GateDefinition Definition => definition;
        public static event Action<TrackGate, GameObject> Triggered;

        public void Configure(GateDefinition value) => definition = value;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!definition || consumed || (vehicleLayers.value & (1 << other.gameObject.layer)) == 0) return;
            var body = other.attachedRigidbody;
            GameObject participant = body ? body.gameObject : other.gameObject;
            Triggered?.Invoke(this, participant);
            if (definition.OneShot) consumed = true;
        }
    }
}
