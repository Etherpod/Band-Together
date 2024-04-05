using System.Linq;
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
            DebugMenu.Instance.TeleportPlayer("GhirdShrubbery");
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