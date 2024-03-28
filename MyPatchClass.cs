using System.Linq;
using HarmonyLib;
using OWML.ModHelper;

namespace BandTogether;

[HarmonyPatch]
public class MyPatchClass
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(GhostSensors), nameof(GhostSensors.FixedUpdate_Sensors))]
	public static void FlashLightOwlk(GhostSensors __instance)
	{
		__instance._data.sensor.isIlluminatedByPlayer = __instance._data.sensor.isIlluminated;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnSnapPlayerNeck))]
	public static void OwlkSnap(GhostGrabController __instance)
	{
		if (!Locator.GetDeathManager().IsPlayerDying() && !Locator.GetDeathManager().IsPlayerDead())
		{
			Locator.GetDeathManager().KillPlayer(DeathType.CrushedByElevator);
		}
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
		};

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
					ModMain.WriteMessage("Condition: " + condition);
					ModMain.WriteMessage(sharedInstance.GetConditionState(condition));
					// ModMain.WriteMessage($"found condition value: {sharedInstance.GetConditionState(condition)}");
					return sharedInstance.GetConditionState(condition);
				}

				// ModMain.WriteMessage("condition not found");
				return false;
			});
	}
}