using System;
using UnityEngine;

namespace BandTogether;
public class NoSuitDoor : MonoBehaviour
{
	[SerializeField] EclipseDoorController doorController;

	bool suitOff;

	private void Update()
	{
		if (!suitOff && !Locator.GetPlayerSuit().IsWearingSuit())
		{
			suitOff = true;
			doorController.CallOpenEvent();
		}
		else if (suitOff && Locator.GetPlayerSuit().IsWearingSuit())
		{
			suitOff = false;
			doorController.CallCloseEvent();
        }
	}
}
