using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineBasicMultiChannelPerlin))]
public class CinemachineCameraManager : MonoBehaviour {
    public static CinemachineCameraManager Instance;
    private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin _cameraShake;
    private float _currentShakeTimeLeft = 0f;
    private Coroutine _currentShakeCoroutine = null;
    // Start is called before the first frame update
    void Awake() {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        _cameraShake = GetComponent<CinemachineBasicMultiChannelPerlin>();
        Instance = this;
    }

    // Update is called once per frame
    public void Shake(float amplitude = 1f, float duration = .3f, float frequency = 3f) {
        if (_currentShakeCoroutine != null) StopCoroutine(_currentShakeCoroutine);

        _cameraShake.AmplitudeGain = System.Math.Max(amplitude, _cameraShake.AmplitudeGain);
        _cameraShake.FrequencyGain = System.Math.Max(frequency, _cameraShake.FrequencyGain);
        _currentShakeTimeLeft = duration;
        _currentShakeCoroutine = StartCoroutine(StopShakingCoroutine());
    }

    private IEnumerator StopShakingCoroutine() {
        while (_currentShakeTimeLeft > 0) {
            _currentShakeTimeLeft -= Time.deltaTime;
            yield return null;
        }
        _cameraShake.AmplitudeGain = 0;
        _cameraShake.FrequencyGain = 0;
    }
}
