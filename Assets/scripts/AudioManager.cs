using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class handles playing audio in the game
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Singleton instance of the AudioManager
    [SerializeField]
    private AudioSource audioSource; // Reference to the AudioSource component
    [SerializeField]
    private AudioClip[] audioClips; // Array of audio clips to be played

    private static float volume = 1; // Default volume

    void Awake()
    {
        Instance = this; // Initialize the singleton instance
    }

    // Play an audio clip with the specified ID
    public void PlayAudioClip(int clipID)
    {
        audioSource.PlayOneShot(audioClips[clipID]);
    }

    // Play an audio clip with the specified ID and volume
    public void PlayAudioClip(int clipID, float vol)
    {
        audioSource.PlayOneShot(audioClips[clipID], vol);
    }
}
