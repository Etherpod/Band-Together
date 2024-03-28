using UnityEngine;

namespace BandTogether;

public class Shrubbery : OWItem
{
    public static readonly ItemType ItemType = (ItemType)256;

    public override string GetDisplayName() =>
      """
    The Sacred Shrubbery, Guardian of Light, Protector of the Faithful, Beacon of Hope,
    Harbinger of Peace, Fountain of Wisdom, Arbiter of Justice, Patron of Tranquility,
    Sentinel of the Sacred Grove, Keeper of Secrets, Bearer of Blessings, Herald of Harmony,
    Eternal Patron of Transcendental Unity, Sacred Arboreal Beacon of Celestial Illumination,
    Ethereal Arboreal Custodian of Harmonious Existence, Divine Guardian of Cosmic Order,
    Celestial Patron of Transcendental Bliss, Eternal Arboreal Sentinel of Universal Harmony
    """;

    public override void Awake()
    {
        base.Awake();
        _type = ItemType;
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public override void SocketItem(Transform socketTransform, Sector sector)
    {
        base.SocketItem(socketTransform, sector);
        transform.localScale = Vector3.one;
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        base.DropItem(position, normal, parent, sector, customDropTarget);
        transform.localScale = Vector3.one;
    }
}