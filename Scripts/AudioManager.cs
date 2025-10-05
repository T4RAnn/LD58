using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Амбиент")]
    public AudioClip ambientClip;
    [Range(0f, 1f)] public float ambientVolume = 0.5f;

    [Header("Звуки эффектов")]
    public AudioClip cardPickSound;     // взятие карты
    public AudioClip statUpSound;       // повышение стата
    public AudioClip shakeSound;        // тряска
    public AudioClip victorySound;      // победа
    public AudioClip attackSound;       // атака
    public AudioClip deathSound;        // смерть
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource ambientSource;
    private AudioSource sfxSource;

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
            return;
        }

        // Создаём каналы аудио
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.clip = ambientClip;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.playOnAwake = false;
    }

    private void Start()
    {
        PlayAmbient();
    }

    // =======================
    // Методы управления
    // =======================

    public void PlayAmbient()
    {
        if (ambientSource != null && ambientClip != null)
            ambientSource.Play();
    }

    public void StopAmbient()
    {
        if (ambientSource != null)
            ambientSource.Stop();
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null)
            ambientSource.volume = ambientVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    // =======================
    // Проигрывание конкретных эффектов
    // =======================
    public void PlayCardPick() => PlaySFX(cardPickSound);
    public void PlayStatUp() => PlaySFX(statUpSound);
    public void PlayShake() => PlaySFX(shakeSound);
    public void PlayVictory() => PlaySFX(victorySound);
    public void PlayAttack() => PlaySFX(attackSound);
    public void PlayDeath() => PlaySFX(deathSound); // новый метод

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
