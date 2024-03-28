using System;
using UnityEngine;

namespace BandTogether;

[RequireComponent(typeof(OWTriggerVolume))]
[RequireComponent(typeof(AudioSource))]
public class AmbientMusicArea : MonoBehaviour
{
    [SerializeField] float fadeTime;

    OWTriggerVolume trigger;
    AudioSource audio;
    bool fadeIn;
    bool fadeOut;
    float fadeStartTime;
    float lastVolume;
    float maxVolume;

    private void Start()
    {
        trigger = GetComponent<OWTriggerVolume>();
        audio = GetComponent<AudioSource>();

        maxVolume = audio.volume;

        trigger.OnEntry += OnEntry;
        trigger.OnExit += OnExit;
    }

    private void Update()
    {
        if (fadeIn)
        {
            float num = Mathf.InverseLerp(fadeStartTime, fadeStartTime + fadeTime, Time.time);
            audio.volume = Mathf.Lerp(lastVolume, maxVolume, num);
            if (audio.volume >= maxVolume)
            {
                fadeIn = false;
            }
        }
        else if (fadeOut)
        {
            float num = Mathf.InverseLerp(fadeStartTime, fadeStartTime + fadeTime, Time.time);
            audio.volume = Mathf.Lerp(lastVolume, 0f, num);
            if (audio.volume <= 0)
            {
                audio.Stop();
                fadeOut = false;
            }
        }
    }

    private void OnEntry(GameObject obj)
    {
        if (obj.CompareTag("PlayerDetector"))
        {
            audio.time = 0;
            audio.Play();
            lastVolume = audio.volume;
            fadeStartTime = Time.time;
            fadeIn = true;
        }
    }

    private void OnExit(GameObject obj)
    {
        if (obj.CompareTag("PlayerDetector"))
        {
            lastVolume = audio.volume;
            fadeStartTime = Time.time;
            fadeOut = true;
        }
    }

    private void OnDestroy()
    {
        trigger.OnEntry -= OnEntry;
        trigger.OnExit -= OnExit;
    }
}
