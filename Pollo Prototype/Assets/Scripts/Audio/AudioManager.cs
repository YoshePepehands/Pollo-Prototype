using UnityEngine.Audio;
using System;
using UnityEngine;

//The Audio Manager handles generating the audio source and controls the individual settings
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;    //Singleton pattern

    void Awake()
    {
        //Set this copy to static instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        //Dont destroy this instance on scene change
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    private void Start()
    {
        //Play the BGM on loop
        Play("BGM");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }
}
