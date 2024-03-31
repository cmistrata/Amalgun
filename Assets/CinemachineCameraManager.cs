using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineBasicMultiChannelPerlin))]
public class CinemachineCameraManager : MonoBehaviour
{
    public static CinemachineCameraManager Instance;
    private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin cameraShake;
    float currentShakeTimeLeft = 0f;
    private Coroutine currentShakeCoroutine = null;
    // Start is called before the first frame update
    void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        cameraShake = GetComponent<CinemachineBasicMultiChannelPerlin>();
        Instance = this;
    }

    // Update is called once per frame
    public void Shake(float amplitude = 1f, float duration = .3f, float frequency = 3f)
    {
        if (currentShakeCoroutine != null) StopCoroutine(currentShakeCoroutine);

        cameraShake.AmplitudeGain = System.Math.Max(amplitude, cameraShake.AmplitudeGain);
        cameraShake.FrequencyGain = System.Math.Max(frequency, cameraShake.FrequencyGain);
        currentShakeTimeLeft = duration;
        currentShakeCoroutine = StartCoroutine(StopShakingCoroutine());
    }

    private IEnumerator StopShakingCoroutine() {
        while (currentShakeTimeLeft > 0) {
            if (!GameManager.Instance.Paused) {
                currentShakeTimeLeft -= Time.deltaTime;
                yield return null;
            }

        }
        cameraShake.AmplitudeGain = 0;
        cameraShake.FrequencyGain = 0;
    }
}
