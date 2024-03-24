using System;
using UnityEngine;

public class NoSuitDoor
{
	[SerializeField] EclipseDoorController doorController;

	bool suitOff;

	private void Update()
	{
		if (!suitOff && GameObject.Find("ExpeditionGear").GetComponent<SuitPickupVolume>()._containsSuit)
		{
			suitOff = true;
			DialogueConditionManager.SharedInstance.SetConditionState("PLAYER_SUIT_OFF", true);
			doorController.CallOpenEvent();
		}
		else if (suitOff && GameObject.Find("ExpeditionGear").GetComponent<SuitPickupVolume>()._containsSuit)
		{
			suitOff = false;
            DialogueConditionManager.SharedInstance.SetConditionState("PLAYER_SUIT_OFF", false);
			doorController.CallCloseEvent();
        }
	}
}
