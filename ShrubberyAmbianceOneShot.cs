using UnityEngine;

namespace BandTogether;

[RequireComponent(typeof(OWAudioSource))]
[RequireComponent(typeof(OWTriggerVolume))]
public class ShrubberyAmbianceOneShot : MonoBehaviour
{
    private OWAudioSource audio;
    private OWTriggerVolume trigger;

    private void Start()
    {
        audio = gameObject.GetRequiredComponent<OWAudioSource>();
        trigger = gameObject.GetRequiredComponent<OWTriggerVolume>();

        trigger.OnEntry += OnEntry;
    }

    private void OnEntry(GameObject obj)
    {
        if (obj.CompareTag("PlayerDetector") && !ModMain.GetPersistentCondition("BT_SEEN_SHRUB_SHRINE"))
        {
            ModMain.SetPersistentCondition("BT_SEEN_SHRUB_SHRINE", true);
            audio.FadeIn(0f, true, false, 1f);
        }
    }

    private void OnDestroy()
    {
        trigger.OnEntry -= OnEntry;
    }
}
