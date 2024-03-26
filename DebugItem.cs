using UnityEngine;

namespace BandTogether;

public class DebugItem : OWItem
{
  public static readonly ItemType ItemType = (ItemType)1024;
  
  public override string GetDisplayName() => "Debug Item";

  private static readonly string[][] Conditions =
  {
    new []{"NOMAI_VILLAGE_A_TO_DOOR", "NOMAI_VILLAGE_B_TO_DOOR", "GHIRD_VILLAGE_A_TO_DOOR", "GHIRD_VILLAGE_B_TO_DOOR"},
    new []{"CLANS_LEAVE_DOOR"},
    new []{"NOMAI_VILLAGE_A_TO_FIRE", "NOMAI_VILLAGE_B_TO_FIRE", "GHIRD_VILLAGE_A_TO_FIRE", "GHIRD_VILLAGE_B_TO_FIRE"},
  };

  private int i = 0;

  public override void Awake()
  {
    base.Awake();
    _type = ItemType;

    onPickedUp += OnYoink;
  }

  private void OnYoink(OWItem item)
  {
    if (i >= Conditions.Length) return;
    
    foreach (var s in Conditions[i])
    {
      DialogueConditionManager.SharedInstance.SetConditionState(s, true);
      ModMain.Instance.ModHelper.Console.WriteLine($"set condition: {s}");
    }

    i = (i + 1) % Conditions.Length;
  }
}