using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AudioPlayback : MonoBehaviour
{
    [SerializeField] AudioClip[] _playerShotClips;
    [SerializeField] AudioClip[] _enemyExplosionClips;

    public static AudioPlayback Instance { get; private set; }

    AudioSource _playerShotSource;
    AudioSource _enemyExplosionSource;

    public enum SFX
    {
        None,
        PlayerShot,
        EnemyExplosion,
    }

    void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError(GetType().ToString() + "." + MethodBase.GetCurrentMethod().Name + " - Singleton Instance already exists!");
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        // Create the AudioSource objects and add as chidren
        CreateAudioSourceChild(out _playerShotSource, "PlayerShotSource", 0.5f);
        CreateAudioSourceChild(out _enemyExplosionSource, "EnemyExplosionSource");
    }

    void CreateAudioSourceChild(out AudioSource audioSource, string audioSourceName, float volume = 1.0f)
    {
        GameObject audioSourceGO = new GameObject(audioSourceName);
        audioSource = audioSourceGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.transform.parent = transform;
        audioSource.volume = volume;
    }

    void PlayRandomSoundFromClips(AudioSource audioSource, AudioClip[] audioClips, bool stopIfPlaying = true)
    {
        if(audioClips.Length <= 0)
        {
            return;
        }

        if(stopIfPlaying && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Randomly select a clip, set the clip in the AudioSource, then play it
        AudioClip audioClip = audioClips[Random.Range(0, audioClips.Length)];
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void PlaySound(SFX sfx)
    {
        switch(sfx)
        {
            case SFX.PlayerShot: PlayRandomSoundFromClips(_playerShotSource, _playerShotClips); break;
            case SFX.EnemyExplosion: PlayRandomSoundFromClips(_enemyExplosionSource, _enemyExplosionClips); break;
            default: break;
        }
    }
}
