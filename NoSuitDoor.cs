using System;
using UnityEngine;

public class NoSuitDoor : MonoBehaviour
{
	[SerializeField] EclipseDoorController doorController;

	bool suitOff;

	private void Update()
	{
		if (!suitOff && GameObject.Find("ExpeditionGear").GetComponent<SuitPickupVolume>()._containsSuit)
		{
			suitOff = true;
			doorController.CallOpenEvent();
		}
		else if (suitOff && !GameObject.Find("ExpeditionGear").GetComponent<SuitPickupVolume>()._containsSuit)
		{
			suitOff = false;
			doorController.CallCloseEvent();
        }
	}
}
