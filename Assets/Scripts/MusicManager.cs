using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public AudioClip EasyMusic;
    public AudioClip MediumMusic;
    public AudioClip HardMusic;

    [HideInInspector]
    public static MusicManager Instance;
    private AudioSource audioSource;
    private int Difficulty = 0;
    private int NextBar = 0;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void QueueSongStart(int difficulty)
    {
        if (audioSource.clip == null)
        {
            audioSource.clip = EasyMusic;
        }
        var bars = 8;
        var barLength = (audioSource.clip.length / bars);
        // The songs are divided into 8 bars, start a new song after that bar.
        var bar = (int)(audioSource.time / barLength);
        var timeRemaining = barLength - (audioSource.time % (barLength));

        NextBar = (bar + 1) % bars;
        Difficulty = difficulty;
        Invoke("StartSong", timeRemaining);
    }

    private void StartSong()
    {
        var clip = Difficulty == 0 ? EasyMusic : Difficulty == 1 ? MediumMusic : HardMusic;
        audioSource.clip = clip;
        audioSource.time = (audioSource.clip.length / 8) * NextBar;
        audioSource.Play();
    }

    public void RestartEasySong()
    {
        audioSource.clip = EasyMusic;
        audioSource.time = 0;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
