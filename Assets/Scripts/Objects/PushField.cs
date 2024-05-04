using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PushField : MonoBehaviour {
    public float ForceMagnitude = 1000f;
    public Vector3 ForceDirection;
    private void OnTriggerStay(Collider other) {
        other.attachedRigidbody.AddForce(ForceDirection * ForceMagnitude);
    }
}
