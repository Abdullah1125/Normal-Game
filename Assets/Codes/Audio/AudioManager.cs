using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Speaker(Hoparlörler)")]
    public AudioSource sfxSource; 

    [Header("Audio Files(Ses Dosyalarý)")]
    public AudioClip jumpSound;
    public AudioClip dieSound;
    public AudioClip buttonSound;
    public AudioClip keySound;
    public AudioClip doorPassSound;
    public AudioClip slidingDoorSound;

    void Awake()
    {
        
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

      
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    
    public static void PlaySFX(AudioClip clip)
    {
        if (clip != null && instance != null && instance.sfxSource != null)
        {
           
            instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
            instance.sfxSource.PlayOneShot(clip);
        }
    }
}