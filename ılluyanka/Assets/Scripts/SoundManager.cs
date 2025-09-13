using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source Settings")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float uiVolume = 1f;

    [Header("Player Sounds")]
    [SerializeField] private AudioClip[] playerAttackSounds; // 0: punch, 1: kick, 2: mmakick, 3: heavyAttack1, 4: heavyAttack2
    [SerializeField] private AudioClip[] playerDashSounds; // 0: dash start, 1: dash impact
    [SerializeField] private AudioClip[] playerHitSounds; // 0: light hit, 1: heavy hit
    [SerializeField] private AudioClip[] playerDeathSounds;

    [Header("Enemy Sounds")]
    [SerializeField] private EnemySoundData[] enemySounds;

    [System.Serializable]
    public class EnemySoundData
    {
        public EnemyType enemyType;
        public AudioClip[] attackSounds;
        public AudioClip[] hitSounds;
        public AudioClip[] deathSounds;
        public AudioClip[] idleSounds;
        public AudioClip[] specialSounds; // Özel yetenekler için (dragon bomb, mutant jump, wizard fireball)
    }

    public enum EnemyType
    {
        Dragon,
        Mutant,
        Wizard,
        Skeleton // EnemyBase için genel iskelet düşmanı
    }

    private void Awake()
    {
        // Singleton pattern
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
        // Audio source'ları oluştur eğer yoksa
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (uiSource == null)
        {
            GameObject uiObj = new GameObject("UISource");
            uiObj.transform.SetParent(transform);
            uiSource = uiObj.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
        }

        UpdateVolumes();
    }

    private void Update()
    {
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
        if (uiSource != null)
            uiSource.volume = uiVolume * masterVolume;
    }

    #region Player Sound Methods

    public void PlayPlayerAttackSound(int attackType)
    {
        if (playerAttackSounds == null || playerAttackSounds.Length == 0) return;
        
        int index = Mathf.Clamp(attackType, 0, playerAttackSounds.Length - 1);
        PlaySFX(playerAttackSounds[index]);
    }

    public void PlayPlayerDashSound(bool isStart)
    {
        if (playerDashSounds == null || playerDashSounds.Length < 2) return;
        
        int index = isStart ? 0 : 1;
        PlaySFX(playerDashSounds[index]);
    }

    public void PlayPlayerHitSound(bool isHeavy = false)
    {
        if (playerHitSounds == null || playerHitSounds.Length < 2) return;
        
        int index = isHeavy ? 1 : 0;
        PlaySFX(playerHitSounds[index]);
    }

    public void PlayPlayerDeathSound()
    {
        if (playerDeathSounds != null && playerDeathSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, playerDeathSounds.Length);
            PlaySFX(playerDeathSounds[randomIndex]);
        }
    }

    #endregion

    #region Enemy Sound Methods

    public void PlayEnemyAttackSound(EnemyType enemyType, int attackIndex = 0)
    {
        EnemySoundData soundData = GetEnemySoundData(enemyType);
        if (soundData != null && soundData.attackSounds != null && soundData.attackSounds.Length > 0)
        {
            int index = Mathf.Clamp(attackIndex, 0, soundData.attackSounds.Length - 1);
            PlaySFX(soundData.attackSounds[index]);
        }
    }

    public void PlayEnemyHitSound(EnemyType enemyType, int hitIndex = 0)
    {
        EnemySoundData soundData = GetEnemySoundData(enemyType);
        if (soundData != null && soundData.hitSounds != null && soundData.hitSounds.Length > 0)
        {
            int index = Mathf.Clamp(hitIndex, 0, soundData.hitSounds.Length - 1);
            PlaySFX(soundData.hitSounds[index]);
        }
    }

    public void PlayEnemyDeathSound(EnemyType enemyType)
    {
        EnemySoundData soundData = GetEnemySoundData(enemyType);
        if (soundData != null && soundData.deathSounds != null && soundData.deathSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, soundData.deathSounds.Length);
            PlaySFX(soundData.deathSounds[randomIndex]);
        }
    }

    public void PlayEnemyIdleSound(EnemyType enemyType)
    {
        EnemySoundData soundData = GetEnemySoundData(enemyType);
        if (soundData != null && soundData.idleSounds != null && soundData.idleSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, soundData.idleSounds.Length);
            PlaySFX(soundData.idleSounds[randomIndex]);
        }
    }

    public void PlayEnemySpecialSound(EnemyType enemyType, int specialIndex = 0)
    {
        EnemySoundData soundData = GetEnemySoundData(enemyType);
        if (soundData != null && soundData.specialSounds != null && soundData.specialSounds.Length > 0)
        {
            int index = Mathf.Clamp(specialIndex, 0, soundData.specialSounds.Length - 1);
            PlaySFX(soundData.specialSounds[index]);
        }
    }

    #endregion

    #region Generic Sound Methods

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void PlayUISound(AudioClip clip)
    {
        if (clip != null && uiSource != null)
        {
            uiSource.PlayOneShot(clip);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    #endregion

    #region Helper Methods

    private EnemySoundData GetEnemySoundData(EnemyType enemyType)
    {
        if (enemySounds == null) return null;

        foreach (EnemySoundData soundData in enemySounds)
        {
            if (soundData.enemyType == enemyType)
            {
                return soundData;
            }
        }
        return null;
    }

    // Volume control methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test All Player Sounds")]
    public void TestAllPlayerSounds()
    {
        StartCoroutine(TestPlayerSoundsCoroutine());
    }

    private IEnumerator TestPlayerSoundsCoroutine()
    {
        Debug.Log("Testing Player Sounds...");
        
        // Test attack sounds
        for (int i = 0; i < playerAttackSounds.Length; i++)
        {
            PlayPlayerAttackSound(i);
            Debug.Log("Playing attack sound " + i);
            yield return new WaitForSeconds(1f);
        }
        
        // Test dash sounds
        PlayPlayerDashSound(true);
        Debug.Log("Playing dash start sound");
        yield return new WaitForSeconds(1f);
        
        PlayPlayerDashSound(false);
        Debug.Log("Playing dash impact sound");
        yield return new WaitForSeconds(1f);
        
        // Test hit sounds
        PlayPlayerHitSound(false);
        Debug.Log("Playing light hit sound");
        yield return new WaitForSeconds(1f);
        
        PlayPlayerHitSound(true);
        Debug.Log("Playing heavy hit sound");
        yield return new WaitForSeconds(1f);
        
        Debug.Log("Player sound test completed!");
    }

    [ContextMenu("Test All Enemy Sounds")]
    public void TestAllEnemySounds()
    {
        StartCoroutine(TestEnemySoundsCoroutine());
    }

    private IEnumerator TestEnemySoundsCoroutine()
    {
        Debug.Log("Testing Enemy Sounds...");
        
        foreach (EnemyType enemyType in System.Enum.GetValues(typeof(EnemyType)))
        {
            Debug.Log("Testing " + enemyType + " sounds...");
            
            PlayEnemyAttackSound(enemyType);
            yield return new WaitForSeconds(1f);
            
            PlayEnemyHitSound(enemyType);
            yield return new WaitForSeconds(1f);
            
            PlayEnemySpecialSound(enemyType);
            yield return new WaitForSeconds(1f);
        }
        
        Debug.Log("Enemy sound test completed!");
    }

    #endregion
}
