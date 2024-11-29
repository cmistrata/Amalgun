using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;
    public void Awake() {
        Instance = this;
    }

    public void PlayCellDestroy() {
        var audioSource = GetAudioSourceWithName("Part_destroy");
        audioSource.Play();
    }

    public void PlayEnemyDestroy() {
        var audioSource = GetAudioSourceWithName("Enemy_destroy");
        audioSource.Play();
    }

    public void PlayNeutralizeSound(float pitch = 1) {
        var audioSource = GetAudioSourceWithName("neutralize");
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

    public void PlayAbsorbSound() {
        var audioSource = GetAudioSourceWithName("amalgun_merging");
        audioSource.pitch = 1.6f + Random.Range(-.2f, .2f);
        audioSource.Play();
    }

    public void PlayMergedSound() {
        var audioSource = GetAudioSourceWithName("small_arcade_explosion");
        audioSource.pitch = 2.5f + Random.Range(-.2f, .2f);
        audioSource.time = .35f;
        audioSource.Play();
    }

    public void PlayAttachSound() {
        var audioSource = GetAudioSourceWithName("attach");
        audioSource.pitch = 1.6f + Random.Range(-.3f, .3f);
        audioSource.time = 0.2f;
        audioSource.Play();
    }

    private AudioSource GetAudioSourceWithName(string name) {
        var audioSources = GetComponents<AudioSource>();
        foreach (var audioSource in audioSources) {
            if (audioSource.clip.name == name) {
                return audioSource;
            }
        }
        Debug.LogError($"Couldn't get sound effect with name ${name}");
        return null;
    }
}
