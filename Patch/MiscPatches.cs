using System.Linq;
using System.Runtime.CompilerServices;
using BandTogether.Debug;
using HarmonyLib;
using UnityEngine;

namespace BandTogether.Patch;

[HarmonyPatch]
public class MiscPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(GhostSensors), nameof(GhostSensors.FixedUpdate_Sensors))]
	public static void FlashLightOwlk(GhostSensors __instance)
	{
		__instance._data.sensor.isIlluminatedByPlayer = __instance._data.sensor.isIlluminated;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnSnapPlayerNeck))]
	public static bool GhostGrabController_OnSnapPlayerNeck_Prefix(GhostGrabController __instance)
	{
        if (!Locator.GetDeathManager().IsPlayerDying() && !Locator.GetDeathManager().IsPlayerDead())
        {
            Locator.GetDreamWorldController().ExitDreamWorld(DreamWakeType.NeckSnapped);
			RespawnTeleport();
            __instance.ReleasePlayer();
			GhostBrain brain = __instance.GetComponentInParent<GhostBrain>();
            brain.ChangeAction(null);
			brain._data.OnPlayerExitDreamWorld();
            ReticleController.Show();
            Locator.GetPromptManager().SetPromptsVisible(true);
			Locator.GetDreamWorldController()._playerCamEffectController.OpenEyes(0.5f, false);
        }

        __instance.enabled = false;

		return false;
	}

	private static void RespawnTeleport()
	{
        var playerBody = Locator.GetPlayerBody();
        var destination = ReferenceLocator.GetPlayerRespawnPoint();
        var planetBody = ModMain.Instance.Planet.GetComponent<OWRigidbody>();

        var targetRotation = destination.rotation;
        var targetPosition = destination.position + 2 * (targetRotation * Vector3.up);
        var targetVelocity = planetBody.GetVelocity();

        playerBody.SetPosition(targetPosition);
        playerBody.SetRotation(targetRotation);
        playerBody.SetVelocity(targetVelocity);

        TheDivineThrone throneSocket = ReferenceLocator.GetShrubSocketThrone();
		ItemTool itemTool = Object.FindObjectOfType<ItemTool>();
        Shrubbery shrub = ReferenceLocator.GetShrubbery();

        if (itemTool.GetHeldItemType() == shrub.GetItemType())
		{
			itemTool.SocketItem(throneSocket);
            ModMain.SetCondition("HAS_SHRUBBERY", false);
        }
        else if (!throneSocket.IsSocketOccupied())
		{
            throneSocket.PlaceIntoSocket(shrub);
		}

        shrub.transform.localScale = Vector3.one;
		throneSocket.EnableInteraction(true);
		ReferenceLocator.GetShrubSocketNomai().EnableInteraction(false);

        SacredEntrywayTrigger entryway = ReferenceLocator.GetSacredEntryway();
        entryway.ForceSetEnabled(true);
		//ReferenceLocator.GetGhirdVillageBDarkZone().OnExit(Locator.GetPlayerDetector());

		CageElevator elevator = ReferenceLocator.GetGhirdVillageBElevator();
		elevator._currentDestinationIdx = elevator._destinations.Length - 1;
		elevator._ghostInterface.SetStartingPosition(true);
		elevator.SetReached(true, true, true);
    }

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DialogueNode), nameof(DialogueNode.EntryConditionsSatisfied))]
	public static void DialogueEntryConditionsSatisfied(DialogueNode __instance, ref bool __result)
	{
		// ModMain.WriteMessage("entry condition patch");

		if (__instance._listEntryCondition.Count == 0)
		{
			// ModMain.WriteMessage("no entry conditions");
			__result = false;
			return;
		}

		;

		var sharedInstance = DialogueConditionManager.SharedInstance;
		__result = __instance._listEntryCondition
			.All(condition =>
			{
				// ModMain.WriteMessage($"checking condition: {condition}");

				if (PlayerData.PersistentConditionExists(condition))
				{
					// ModMain.WriteMessage($"found persistent condition value: {PlayerData.GetPersistentCondition(condition)}");
					return PlayerData.GetPersistentCondition(condition);
				}

				if (sharedInstance.ConditionExists(condition))
				{
					// ModMain.WriteMessage($"found condition value: {sharedInstance.GetConditionState(condition)}");
					return sharedInstance.GetConditionState(condition);
				}

				// ModMain.WriteMessage("condition not found");
				return false;
			});
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DeathManager), nameof(DeathManager.FinishEscapeTimeLoopSequence))]
	public static void CallCampfireEnd(DeathManager __instance)
	{
		if (!__instance._escapedTimeLoopSequenceComplete && !ModMain.Instance.inEndSequence)
		{
			__instance._escapedTimeLoopSequenceComplete = true;
			ModMain.Instance.inEndSequence = true;
			ModMain.Instance.OnTriggerCampfireEnd();
		}
	}
}