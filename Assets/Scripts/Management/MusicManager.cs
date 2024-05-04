using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour {
    public AudioClip EasyMusic;
    public AudioClip MediumMusic;
    public AudioClip HardMusic;

    [HideInInspector]
    public static MusicManager Instance;
    private AudioSource _audioSource;
    private int _difficulty = 0;
    private int _nextBar = 0;

    private void Awake() {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(this);
    }

    public void QueueSongStart(int difficulty) {
        if (_audioSource.clip == null) {
            _audioSource.clip = EasyMusic;
        }
        var bars = 8;
        var barLength = (_audioSource.clip.length / bars);
        // The songs are divided into 8 bars, start a new song after that bar.
        var bar = (int)(_audioSource.time / barLength);
        var timeRemaining = barLength - (_audioSource.time % (barLength));

        _nextBar = (bar + 1) % bars;
        _difficulty = difficulty;
        Invoke(nameof(StartSong), timeRemaining);
    }

    private void StartSong() {
        var clip = _difficulty == 0 ? EasyMusic : _difficulty == 1 ? MediumMusic : HardMusic;
        _audioSource.clip = clip;
        _audioSource.time = (_audioSource.clip.length / 8) * _nextBar;
        _audioSource.Play();
    }

    public void RestartEasySong() {
        _audioSource.clip = EasyMusic;
        _audioSource.time = 0;
        _audioSource.Play();
    }

    public void StopMusic() {
        _audioSource.Stop();
    }
}
