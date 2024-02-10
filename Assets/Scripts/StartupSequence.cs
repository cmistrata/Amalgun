using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartupSequence : MonoBehaviour
{
    public Text AmalgaText;
    public Text InstructionText;
    public GameObject StarterPart;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DisplayAmalgaText", .3f);
    }

    void DisplayAmalgaText() {
        AmalgaText.gameObject.SetActive(true);
        CameraManager.Instance.ShakeCamera(1f, 1f);
        AudioManager.Instance.PlayPartDestroy();
        Invoke("DisplayInstructions", 1.5f);
    }

    // Update is called once per frame
    void DisplayInstructions() {
        InstructionText.gameObject.SetActive(true);
        AudioManager.Instance.PlayUISound();
        Invoke("DisplayStarterPart", 1f);
    }

    void DisplayStarterPart() {
        StarterPart.SetActive(true);
        AudioManager.Instance.PlayUISound(1.5f);
    }
}
