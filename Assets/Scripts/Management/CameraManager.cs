using System;
using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    // Transform of the GameObject you want to shake
    public GameObject Camera;

    # region Shaking logic
    private float _currentShakeTimeLeft = 0f;

    // A measure of magnitude for the shake. Tweak based on your preference
    private float _currentShakeMagnitude = 0f;

    private Coroutine _currentShakeCoroutine = null;
    # endregion

    public static CameraManager Instance;

    public SpriteRenderer DamageFilter;

    public Transform Focus;
    private const float _maxXPos = (Globals.ArenaWidth / 2) - 9.5f;
    private const float _maxZPos = (Globals.ArenaHeight / 2) - 6f;
    private Vector3 _offset = new Vector3(0, 9.4f, -1.64f);

    public void Awake() {
        Instance = this;
    }

    public void ShakeCamera(float shakeMagnitude, float shakeDuration) {
        if (_currentShakeCoroutine != null) StopCoroutine(_currentShakeCoroutine);

        _currentShakeMagnitude = Math.Max(shakeMagnitude, _currentShakeMagnitude);
        _currentShakeTimeLeft = Math.Max(_currentShakeTimeLeft, shakeDuration);
        _currentShakeCoroutine = StartCoroutine(ShakeCameraCoroutine());
    }

    private IEnumerator ShakeCameraCoroutine() {
        while (_currentShakeTimeLeft > 0) {
            Camera.transform.position += UnityEngine.Random.insideUnitSphere * _currentShakeMagnitude;
            _currentShakeTimeLeft -= Time.deltaTime;
            yield return null;
        }
        _currentShakeMagnitude = 0;
    }

    public void FlashDamageFilter() {
        DamageFilter.gameObject.SetActive(true);
        Invoke(nameof(DeactivateDamageFilter), .1f);
    }

    private void DeactivateDamageFilter() {
        DamageFilter.gameObject.SetActive(false);
    }

    void Update() {
        // Have the camera follow the player
        if (Focus != null) {
            float xPos = Mathf.Clamp(Focus.position.x, -_maxXPos, _maxXPos);
            float zPos = Mathf.Clamp(Focus.position.z, -_maxZPos, _maxZPos);
            Camera.transform.position = new Vector3(xPos, 0, zPos) + _offset;
        }
    }
}
