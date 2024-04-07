using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using BandTogether.Util;
using HarmonyLib;

namespace BandTogether.Patch;

[HarmonyPatch]
public class DialoguePatches
{
	private static readonly ThreadLocal<IList<string>> PersistentConditionsToSet = new();
	private static readonly ThreadLocal<IList<string>> PersistentConditionsToDisable = new();

	private static readonly IDictionary<DialogueNode, DialogueNodeEnhancements> NodeEnhancements =
		new Dictionary<DialogueNode, DialogueNodeEnhancements>();

	[HarmonyPostfix]
	[HarmonyPatch(typeof(XContainer), nameof(XContainer.Element))]
	public static void CapturePersistentConditions(XContainer __instance, XName name)
	{
		if (name == "SetPersistentCondition")
		{
			// ModMain.WriteDebugMessage("capturing conditions to set");
			PersistentConditionsToSet.Value = __instance
				.Elements("SetPersistentCondition")
				.Select(element => element.Value)
				.ToList();
		}
		else if (name == "DisablePersistentCondition")
		{
			// ModMain.WriteDebugMessage("capturing conditions to disable");
			PersistentConditionsToDisable.Value = __instance
				.Elements("DisablePersistentCondition")
				.Select(element => element.Value)
				.ToList();
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DialogueNode), nameof(DialogueNode.PersistentConditionToSet), MethodType.Setter)]
	public static void LinkNodeToPersistentConditionsToSet(DialogueNode __instance)
	{
		// ModMain.WriteDebugMessage("linking dialogue node for set");
		if (!NodeEnhancements.TryGetValue(__instance, out var enhancements))
		{
			enhancements = new DialogueNodeEnhancements();
			NodeEnhancements[__instance] = enhancements;
		}

		enhancements.PersistentConditionsToSet.AddAll(PersistentConditionsToSet.Value);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DialogueNode), nameof(DialogueNode.PersistentConditionToDisable), MethodType.Setter)]
	public static void LinkNodeToPersistentConditionsToDisable(DialogueNode __instance)
	{
		// ModMain.WriteDebugMessage("linking dialogue node for disable");
		if (!NodeEnhancements.TryGetValue(__instance, out var enhancements))
		{
			enhancements = new DialogueNodeEnhancements();
			NodeEnhancements[__instance] = enhancements;
		}

		enhancements.PersistentConditionsToDisable.AddAll(PersistentConditionsToDisable.Value);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DialogueNode), nameof(DialogueNode.SetNodeCompleted))]
	public static void SetConditionsWhenNodeComplete(DialogueNode __instance)
	{
		if (!NodeEnhancements.TryGetValue(__instance, out var enhancements)) return;

		ModMain.WriteDebugMessage($"persistent conditions> {enhancements}");

		enhancements
			.PersistentConditionsToSet
			.ForEach(condition => ModMain.SetPersistentCondition(condition, true));

		enhancements
			.PersistentConditionsToDisable
			.ForEach(condition => ModMain.SetPersistentCondition(condition, false));
	}
}

public class DialogueNodeEnhancements
{
	public IList<string> PersistentConditionsToSet { get; } = new List<string>();
	public IList<string> PersistentConditionsToDisable { get; } = new List<string>();

	public override string ToString()
	{
		var toSet = PersistentConditionsToSet.Join(",");
		var toDisable = PersistentConditionsToDisable.Join(",");
		return $"set: [{toSet}] | disable: [{toDisable}]";
	}
}