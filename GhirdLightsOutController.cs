using System;
using UnityEngine;

namespace BandTogether;
public class GhirdLightsOutController : MonoBehaviour
{
    [SerializeField] GameObject[] propGhirds;
    [SerializeField] GhostBrain[] chaseGhirds;
    [SerializeField] GameObject lightsParent;

    bool lightsOut = false;

    private void Update()
    {
        if (lightsOut && PlayerData.GetPersistentCondition("FINISH_STEAL_QUEST"))
        {
            lightsOut = false;
            LightsOn();
        }
        if (lightsOut || PlayerData.GetPersistentCondition("FINISH_STEAL_QUEST")) return;
        
        if (PlayerData.GetPersistentCondition("START_STEAL_QUEST"))
        {
            lightsOut = true;
            LightsOut();
        }
    }

    public void InitializeGhirds()
    {
        foreach (GameObject propGhird in propGhirds)
        {
            propGhird.SetActive(true);
        }

        ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
        {
            foreach (GhostBrain ghostBrain in chaseGhirds)
            {
                ghostBrain.gameObject.SetActive(false);
            }

            if (PlayerData.GetPersistentCondition("START_STEAL_QUEST") && !PlayerData.GetPersistentCondition("FINISH_STEAL_QUEST"))
            {
                lightsOut = true;
                LightsOut();
            }
        });
    }

    public void LightsOut()
    {
        FindObjectOfType<TheDivineThrone>().EnableInteraction(true);

        foreach (Light light in lightsParent.GetComponentsInChildren<Light>())
        {
            light.enabled = false;
        }
        foreach (OWEmissiveRenderer renderer in lightsParent.GetComponentsInChildren<OWEmissiveRenderer>())
        {
            renderer.SetEmissiveScale(0f);
        }
        foreach (GameObject propGhird in propGhirds)
        {
            propGhird.SetActive(false);
        }
        foreach (GhostBrain ghostBrain in chaseGhirds)
        {
            ghostBrain.gameObject.SetActive(true);
            ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                ghostBrain.enabled = true;
                ghostBrain.OnEnterDreamWorld();
                ghostBrain.EscalateThreatAwareness(GhostData.ThreatAwareness.SomeoneIsInHere);
            });
        }
    }

    public void LightsOn()
    {
        foreach (Light light in lightsParent.GetComponentsInChildren<Light>())
        {
            light.enabled = true;
        }
        foreach (OWEmissiveRenderer renderer in lightsParent.GetComponentsInChildren<OWEmissiveRenderer>())
        {
            renderer.SetEmissiveScale(1f);
        }
        foreach (GameObject propGhird in propGhirds)
        {
            propGhird.SetActive(true);
        }
        foreach (GhostBrain ghostBrain in chaseGhirds)
        {
            ghostBrain.gameObject.SetActive(false);
        }
    }
}
