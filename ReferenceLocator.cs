using UnityEngine;

namespace BandTogether;
public class ReferenceLocator : MonoBehaviour
{
    [SerializeField] SacredEntrywayTrigger sacredEntrywayTrigger = null;
    [SerializeField] GhostBrain[] worshipGhosts = null;
    [SerializeField] CageElevator ghirdVillageBElevator = null;
    [SerializeField] TheDivineThrone shrubSocketThrone = null;
    [SerializeField] ShrubberySocketNomai shrubSocketNomai = null;
    [SerializeField] Shrubbery shrubObject = null;
    [SerializeField] SunpostDetector sunpostDetector = null;
    [SerializeField] GhirdLightsOutController lightsOutController = null;
    [SerializeField] DreamLanternItem dreamLantern = null;
    [SerializeField] Transform playerRespawnPoint = null;
    [SerializeField] FlashlightRuleset flashlightRuleset = null;
    [SerializeField] OWAudioSource creditsSong = null;

    private static ReferenceLocator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public static SacredEntrywayTrigger GetSacredEntryway()
    {
        return Instance.sacredEntrywayTrigger;
    }

    public static GhostBrain[] GetWorshipGhosts()
    {
        return Instance.worshipGhosts;
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

    public static OWAudioSource GetCreditsSong()
    {
        return Instance.creditsSong;
    }
}
