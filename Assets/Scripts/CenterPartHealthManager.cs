using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterPartHealthManager : MonoBehaviour
{

    public Sprite[] healthSprites;
    private float previousHealth;

    public Part centerPart;

    SpriteRenderer centerRenderer;

    // Start is called before the first frame update
    void Start()
    {
        if (healthSprites == null || healthSprites.Length < centerPart.MaxHealth)
        {
            Debug.LogError("Health Sprites for center part not set, or not enough sprites set");
        }
        centerRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
        centerRenderer.sprite = healthSprites[Mathf.FloorToInt(centerPart.MaxHealth - .01f)];
        previousHealth = centerPart.MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (centerPart.currentHealth != previousHealth && centerPart.currentHealth > 0)
        {
            centerRenderer.sprite = healthSprites[Mathf.FloorToInt((centerPart.currentHealth - .01f))];
            previousHealth = centerPart.currentHealth;
        }
    }
}
