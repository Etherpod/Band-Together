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
		trigger = GetComponent<OWTriggerVolume>();
		trigger.OnEntry += OnEntry;
		trigger.OnExit += OnExit;
	}

	private void Update()
	{
		if (!doorOpen && PlayerData.GetPersistentCondition("FINISH_COCKPIT_QUEST"))
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
		if (hitObj.transform.parent.name == "Module_Cockpit_Body" 
			&& !PlayerData.GetPersistentCondition("FINISH_COCKPIT_QUEST"))
		{
			ModMain.SetPersistentCondition("GOT_COCKPIT", true);
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.transform.parent.name == "Module_Cockpit_Body"
            && !PlayerData.GetPersistentCondition("FINISH_COCKPIT_QUEST"))
		{
			ModMain.SetPersistentCondition("GOT_COCKPIT", false);
		}
	}
}