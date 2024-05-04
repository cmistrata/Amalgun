using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class LookAtMouse : MonoBehaviour {
    public Transform Spine2;
    public Transform Neck;
    private float _currentYRotation;
    private const float _spineProportion = .2f;
    private const float _rate = 4f;
    private const float _maxRotationMagnitude = 130f;

    void Start() {
        _currentYRotation = transform.rotation.eulerAngles.y;
    }

    void LateUpdate() {
        Vector3 targetLookDirection = Utils.GetMousePosition() - transform.position;
        float newTargetYRotation = Vector3.SignedAngle(transform.forward, targetLookDirection, Vector3.up);
        if (newTargetYRotation > 0) {
            newTargetYRotation = Math.Min(newTargetYRotation, _maxRotationMagnitude);
        }
        else {
            newTargetYRotation = Math.Max(newTargetYRotation, -_maxRotationMagnitude);
        }

        _currentYRotation = Mathf.Lerp(_currentYRotation, newTargetYRotation, Time.deltaTime * _rate);
        float _spine2YRotation = _currentYRotation * _spineProportion;
        float _neckYRotation = _currentYRotation * (1 - _spineProportion);


        Spine2.transform.localRotation = Quaternion.Euler(Spine2.transform.localEulerAngles.x, _spine2YRotation, Spine2.transform.localEulerAngles.z);
        Neck.transform.localRotation = Quaternion.Euler(Neck.transform.localEulerAngles.x, _neckYRotation, Neck.transform.localEulerAngles.z);
    }
}
