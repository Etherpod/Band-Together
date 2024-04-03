using System;
using UnityEngine;

namespace BandTogether;

[RequireComponent(typeof(OWTriggerVolume))]
[RequireComponent(typeof(OWAudioSource))]
public class AmbientMusicArea : MonoBehaviour
{
    [SerializeField] private float fadeTime;

    OWTriggerVolume trigger;
    OWAudioSource audio;

    private void Start()
    {
        trigger = GetComponent<OWTriggerVolume>();
        audio = GetComponent<OWAudioSource>();

        trigger.OnEntry += OnEntry;
        trigger.OnExit += OnExit;
    }

    private void OnEntry(GameObject obj)
    {
        if (obj.CompareTag("PlayerDetector")) audio.FadeIn(fadeTime);
    }

    private void OnExit(GameObject obj)
    {
        if (obj.CompareTag("PlayerDetector")) FadeOut();
    }

    public void FadeOut()
    {
        audio.FadeOut(fadeTime);
    }

    private void OnDestroy()
    {
        trigger.OnEntry -= OnEntry;
        trigger.OnExit -= OnExit;
    }
}
