using UnityEngine;

namespace BandTogether.TheDoor;

public class KeyFragment : OWItem
{
  public static readonly ItemType ItemType = (ItemType)256;

  [SerializeField] private Transform item;

  public override string GetDisplayName() => "Key Fragment";

  public override void Awake()
  {
    base.Awake();
    _type = ItemType;
  }

  public override void PickUpItem(Transform holdTranform)
  {
    base.PickUpItem(holdTranform);
    item.localRotation = Quaternion.Euler(90f, 0f, 0f);
    item.localPosition = new Vector3(0f, -0.4f, 0f);
  }

  public override void SocketItem(Transform socketTransform, Sector sector)
  {
    base.SocketItem(socketTransform, sector);
    item.localRotation = Quaternion.identity;
    item.localPosition = Vector3.zero;
  }

  public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
  {
    base.DropItem(position, normal, parent, sector, customDropTarget);
    item.localRotation = Quaternion.identity;
    item.localPosition = Vector3.zero;
  }
}