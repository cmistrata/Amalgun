using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffectsManager : MonoBehaviour
{
    // Transform of the GameObject you want to shake
    public GameObject Camera;

    # region Shaking logic
    private float currentShakeTimeLeft = 0f;

    // A measure of magnitude for the shake. Tweak based on your preference
    private float currentShakeMagnitude = 0f;

    private Coroutine currentShakeCoroutine = null;

    private Vector3 cameraOffset = Vector3.zero;
    # endregion

    public static CameraEffectsManager Instance;

    public SpriteRenderer DamageFilter;

    public Transform PlayerTransform;

    private Vector3 zOffset = new Vector3(0, 0, -10);

    public void Awake()
    {
        Instance = this;
    }

    public void ShakeCamera(float shakeMagnitude, float shakeDuration)
    {
        if (currentShakeCoroutine != null) StopCoroutine(currentShakeCoroutine);

        currentShakeMagnitude = Math.Max(shakeMagnitude, currentShakeMagnitude);
        currentShakeTimeLeft = Math.Max(currentShakeTimeLeft, shakeDuration);
        currentShakeCoroutine = StartCoroutine(ShakeCameraCoroutine());
    }

    private IEnumerator ShakeCameraCoroutine()
    {
        while (currentShakeTimeLeft > 0)
        {  
            if (!GameManager.Instance.Paused) {
                Camera.transform.position += UnityEngine.Random.insideUnitSphere * currentShakeMagnitude;
                currentShakeTimeLeft -= Time.deltaTime;
                yield return null;
            }
            
        }
        currentShakeMagnitude = 0;
    }

    public void FlashDamageFilter()
    {
        DamageFilter.gameObject.SetActive(true);
        Invoke("DeactivateDamageFilter", .1f);
    }

    private void DeactivateDamageFilter()
    {
        DamageFilter.gameObject.SetActive(false);
    }

    void Update()
    {
        // Have the camera follow the player

        Camera.transform.position = PlayerTransform != null ? new Vector3(PlayerTransform.position.x, PlayerTransform.position.y, -10) : Camera.transform.position;
    }
}
