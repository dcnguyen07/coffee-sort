using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; 

    [Header("🎵 Sound Clips")]
    public AudioClip trayPlacedClip;
    public AudioClip cupJumpClip; 
    public AudioClip trayFullClip;
    public AudioClip cardBoardGoClip;
    public AudioClip backgroundMusic;
    public AudioClip levelFail;
    public AudioClip levelSuccess;
    public AudioClip newSlot;
    public AudioClip clickSfx;
    public AudioClip startGameSfx;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.clip = backgroundMusic;
        musicSource.volume = 0.5f;
        musicSource.Play();
        LoadSettings();
    }

    /// <summary>
    /// play sfx
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    /// <summary>
    /// load sound settings
    /// </summary>
    private void LoadSettings()
    {
        SetMusic(PlayerPrefs.GetInt("Music", 1) == 1);
        SetSound(PlayerPrefs.GetInt("Sound", 1) == 1);
        SetShock(PlayerPrefs.GetInt("Shock", 1) == 1);
    }

    /// <summary>
    /// toggle sound
    /// </summary>
    public void ToggleSound()
    {
        bool isOn = !IsSoundOn();
        PlayerPrefs.SetInt("Sound", isOn ? 1 : 0);
        PlayerPrefs.Save();
        SetSound(isOn);
    }

    /// <summary>
    /// toggle music
    /// </summary>
    public void ToggleMusic()
    {
        bool isOn = !IsMusicOn();
        PlayerPrefs.SetInt("Music", isOn ? 1 : 0);
        PlayerPrefs.Save();
        SetMusic(isOn);
    }

    /// <summary>
    /// toggle shock
    /// </summary>
    public void ToggleShock()
    {
        bool isOn = !IsShockOn();
        PlayerPrefs.SetInt("Shock", isOn ? 1 : 0);
        PlayerPrefs.Save();
        SetShock(isOn);
    }

    /// <summary>
    /// check sound status
    /// </summary>
    public bool IsSoundOn() => PlayerPrefs.GetInt("Sound", 1) == 1;

    /// <summary>
    /// check music status
    /// </summary>
    public bool IsMusicOn() => PlayerPrefs.GetInt("Music", 1) == 1;

    /// <summary>
    /// check shock status
    /// </summary>
    public bool IsShockOn() => PlayerPrefs.GetInt("Shock", 1) == 1;

    /// <summary>
    /// toggle music
    /// </summary>
    private void SetMusic(bool isOn)
    {
        if (musicSource != null)
        {
            musicSource.mute = !isOn;
        }
    }

    /// <summary>
    /// toggle sound
    /// </summary>
    private void SetSound(bool isOn)
    {
        if (sfxSource != null)
        {
            sfxSource.mute = !isOn;
        }
    }

    /// <summary>
    /// toggle shock
    /// </summary>
    private void SetShock(bool isOn)
    {
        if (!isOn)
        {
            Handheld.Vibrate();
        }
    }
}