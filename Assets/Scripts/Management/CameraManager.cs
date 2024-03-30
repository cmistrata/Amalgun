using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Transform of the GameObject you want to shake
    public GameObject Camera;

    # region Shaking logic
    private float currentShakeTimeLeft = 0f;

    // A measure of magnitude for the shake. Tweak based on your preference
    private float currentShakeMagnitude = 0f;

    private Coroutine currentShakeCoroutine = null;
    # endregion

    public static CameraManager Instance;

    public SpriteRenderer DamageFilter;

    public Transform Focus;
    private float _maxXPos = 8f;
    private float _maxZPos = 4f;
    private Vector3 _offset = new Vector3(0, 9.4f, -1.64f);

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
        if (Focus != null) {
            float xPos = Mathf.Clamp(Focus.position.x, -_maxXPos, _maxXPos);
            float zPos = Mathf.Clamp(Focus.position.z, -_maxZPos, _maxZPos);
            Camera.transform.position = new Vector3(xPos, 0, zPos) + _offset;
        }
    }
}
