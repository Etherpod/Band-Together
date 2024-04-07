using UnityEngine;

namespace BandTogether;
public class ReferenceLocator : MonoBehaviour
{
    [SerializeField] SacredEntrywayTrigger sacredEntrywayTrigger;
    [SerializeField] DarkZone ghirdVillageBDarkZone;
    [SerializeField] CageElevator ghirdVillageBElevator;
    [SerializeField] TheDivineThrone shrubSocketThrone;
    [SerializeField] ShrubberySocketNomai shrubSocketNomai;
    [SerializeField] Shrubbery shrubObject;
    [SerializeField] SunpostDetector sunpostDetector;
    [SerializeField] GhirdLightsOutController lightsOutController;
    [SerializeField] DreamLanternItem dreamLantern;
    [SerializeField] Transform playerRespawnPoint;
    [SerializeField] FlashlightRuleset flashlightRuleset;

    private static ReferenceLocator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public static SacredEntrywayTrigger GetSacredEntryway()
    {
        return Instance.sacredEntrywayTrigger;
    }

    public static DarkZone GetGhirdVillageBDarkZone()
    {
        return Instance.ghirdVillageBDarkZone;
    }

    public static CageElevator GetGhirdVillageBElevator()
    {
        return Instance.ghirdVillageBElevator;
    }

    public static TheDivineThrone GetShrubSocketThrone()
    {
        return Instance.shrubSocketThrone;
    }

    public static ShrubberySocketNomai GetShrubSocketNomai()
    {
        return Instance.shrubSocketNomai;
    }

    public static Shrubbery GetShrubbery()
    {
        return Instance.shrubObject;
    }

    public static SunpostDetector GetSunpostDetector()
    {
        return Instance.sunpostDetector;
    }

    public static GhirdLightsOutController GetLightsOutController()
    {
        return Instance.lightsOutController;
    }

    public static DreamLanternItem GetDreamLanternItem()
    {
        return Instance.dreamLantern;
    }

    public static Transform GetPlayerRespawnPoint()
    {
        return Instance.playerRespawnPoint;
    }

    public static FlashlightRuleset GetFlashlightRuleset()
    {
        return Instance.flashlightRuleset;
    }
}
