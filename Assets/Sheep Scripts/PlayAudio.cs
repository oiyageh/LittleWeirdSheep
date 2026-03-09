using UnityEngine;

public class RandomAudioPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;       // The AudioSource component
    public AudioClip[] audioClips;        // List of audio clips to choose from
    public float delayBetweenPlays = 2f;  // Seconds between plays

    private int lastClipIndex = -1;       // Tracks last played clip index

    private void Start()
    {
        // Validate setup
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned!");
            enabled = false;
            return;
        }
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogError("No audio clips assigned!");
            enabled = false;
            return;
        }

        // Start playing audio repeatedly
        InvokeRepeating(nameof(PlayRandomClip), 0f, delayBetweenPlays);
    }

    private void PlayRandomClip()
    {
        if (audioClips.Length == 1)
        {
            // Only one clip, just play it
            audioSource.clip = audioClips[0];
            audioSource.Play();
            return;
        }

        int newIndex;
        do
        {
            newIndex = Random.Range(0, audioClips.Length);
        }
        while (newIndex == lastClipIndex); // Avoid repeating the same clip

        lastClipIndex = newIndex;
        audioSource.clip = audioClips[newIndex];
        audioSource.Play();
    }
}