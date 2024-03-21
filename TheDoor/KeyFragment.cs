using UnityEngine;

namespace OWModJam.TheDoor;

public class KeyFragment : OWItem
{
  public static readonly ItemType ItemType = (ItemType)256;
  
  public override string GetDisplayName() => "Key Fragment";
}