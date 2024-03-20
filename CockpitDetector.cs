using UnityEngine;

namespace OWModJam;

[RequireComponent(typeof(OWTriggerVolume))]
public class CockpitDetector : MonoBehaviour
{
	OWTriggerVolume trigger;

	private void Start()
	{
		ModMain.Instance.ModHelper.Console.WriteLine("Start");
        trigger = GetComponent<OWTriggerVolume>();
		trigger.OnEntry += OnEntry;
		trigger.OnExit += OnExit;
	}

	private void OnEntry(GameObject hitObj)
	{
        if (hitObj.transform.parent.name == "Module_Cockpit_Body")
		{
			//ModMain.Instance.ModHelper.Console.WriteLine("Detected cockpit", OWML.Common.MessageType.Info);
			DialogueConditionManager.SharedInstance.SetConditionState("GOT_COCKPIT", true);
		}
	}

	private void OnExit(GameObject hitObj)
	{
        if (hitObj.transform.parent.name == "Module_Cockpit_Body")
        {
            //ModMain.Instance.ModHelper.Console.WriteLine("Detected cockpit", OWML.Common.MessageType.Info);
            DialogueConditionManager.SharedInstance.SetConditionState("GOT_COCKPIT", true);
        }
    }
}
