using System.Collections;
using UnityEngine;

namespace BandTogether;

[RequireComponent(typeof(OWTriggerVolume))]
public class CockpitDetector : MonoBehaviour
{
	[SerializeField] NomaiInterfaceSlot activateSlot;
	[SerializeField] NomaiMultiPartDoor doorController;

	OWTriggerVolume trigger;
	bool doorOpen = false;

	private void Start()
	{
		ModMain.Instance.ModHelper.Console.WriteLine("Start");
        trigger = GetComponent<OWTriggerVolume>();
		trigger.OnEntry += OnEntry;
		trigger.OnExit += OnExit;
    }

	private void Update()
	{
		if (!doorOpen && DialogueConditionManager.SharedInstance.ConditionExists("FINISH_COCKPIT_QUEST") && DialogueConditionManager.SharedInstance._dictConditions["FINISH_COCKPIT_QUEST"])
		{
			doorOpen = true;
			doorController.Open(activateSlot);
		}
	}

	private void OnDisable()
	{
        trigger.OnEntry -= OnEntry;
        trigger.OnExit -= OnExit;
    }

    private void OnEntry(GameObject hitObj)
	{
        if (hitObj.transform.parent.name == "Module_Cockpit_Body" && !DialogueConditionManager.SharedInstance._dictConditions["FINISH_COCKPIT_QUEST"])
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
