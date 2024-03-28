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
    bool fading;
    float fadeStartTime;

    private void Start()
    {
        trigger = GetComponent<OWTriggerVolume>();
        audio = GetComponent<AudioSource>();

        trigger.OnEntry += OnEntry;
        trigger.OnExit += OnExit;
    }

    private void OnEntry(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            audio.time = 0;
            audio.Play();
        }
    }

    private void OnExit(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            audio.Stop();
        }
    }

    private void OnDestroy()
    {
        trigger.OnEntry -= OnEntry;
        trigger.OnExit -= OnExit;
    }
}
