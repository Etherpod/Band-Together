namespace BandTogether.TheDoor;

public class KeyFragment : OWItem
{
  public static readonly ItemType ItemType = (ItemType)256;

  public override string GetDisplayName() => "Key Fragment";

  public override void Awake()
  {
    base.Awake();
    _type = ItemType;
  }
}