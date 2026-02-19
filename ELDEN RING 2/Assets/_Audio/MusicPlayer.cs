using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip gameMusic;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = gameMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.8f;
    }

    void Start()
    {
        audioSource.Play();
    }
}