using UnityEngine;

namespace BandTogether;

public class Shrubbery : OWItem
{
  public static readonly ItemType ItemType = (ItemType)512;
  
  public override string GetDisplayName() => 
    """
    The Sacred Shrubbery, Guardian of Light, Protector of the Faithful, Beacon of Hope,
    Harbinger of Peace, Fountain of Wisdom, Arbiter of Justice, Patron of Tranquility,
    Sentinel of the Sacred Grove, Keeper of Secrets, Bearer of Blessings, Herald of Harmony,
    Warden of the Wilds, Custodian of Serenity, Embodiment of Divinity, Celestial Arboreal Entity,
    Sovereign of Sanctity, Patron of Purity, Custodian of the Cosmos, Arboreal Avatar of the Divine,
    Eternal Witness of Time, Beacon of Eternity, Luminary of the Heavens, Arboreal Monument of Grace,
    Guardian of the Ethereal Realm, Conduit of Cosmic Energy, Essence of Enlightenment,
    Vessel of the Divine Will, Divine Arboreal Manifestation, Incarnation of Sacred Nature,
    Holistic Guardian of Existence, Arboreal Avatar of the Divine Will,
    Arboreal Custodian of Cosmic Harmony, Seraphic Keeper of Celestial Balance,
    Ethereal Warden of Universal Equilibrium, Arboreal Sentinel of Infinite Wisdom,
    Eternal Guardian of Cosmic Secrets, Celestial Arboreal Luminary of Divine Guidance,
    Sanctified Custodian of Celestial Serenity, Arboreal Emissary of Cosmic Consciousness,
    Eternal Patron of Transcendental Unity, Sacred Arboreal Beacon of Celestial Illumination,
    Ethereal Arboreal Custodian of Harmonious Existence, Divine Guardian of Cosmic Order,
    Celestial Patron of Transcendental Bliss, Eternal Arboreal Sentinel of Universal Harmony
    """;
}