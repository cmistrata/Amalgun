using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public void Awake() {
        Instance = this;
    }

    public void PlayPartDestroy() {
        var audioSource = GetAudioSourceWithName("Part_destroy");
        audioSource.Play();
    }

    public void PlayEnemyDestroy() {
        var audioSource = GetAudioSourceWithName("Enemy_destroy");
        audioSource.Play();
    }

    public void PlayUISound(float pitch = 1) {
        var audioSource = GetAudioSourceWithName("help_text");
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    public void PlayNewWaveSound() {
        var audioSource = GetAudioSourceWithName("new_wave");
        audioSource.Play();
    }

    public void PlayShieldDownSound() {
        var audioSource = GetAudioSourceWithName("shield_deactivate");
        audioSource.Play();
    }

    public void PlayShieldReactivateSound() {
        var audioSource = GetAudioSourceWithName("shield_regen");
        audioSource.Play();
    }

    public void PlayShieldHitSound() {
        var audioSource = GetAudioSourceWithName("shield_hit");
        audioSource.Play();
    }

    public void PlayPlayerDamagedSound(float pitch = 1) {
        var audioSource = GetAudioSourceWithName("center_hit");
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    private AudioSource GetAudioSourceWithName(string name) {
        var audioSources = GetComponents<AudioSource>();
        foreach(var audioSource in audioSources) {
            if (audioSource.clip.name == name) {
                return audioSource;
            }
        }
        Debug.LogError($"Couldn't get sound effect with name ${name}");
        return null;
    }
}
