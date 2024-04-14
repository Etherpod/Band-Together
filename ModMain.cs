using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BandTogether.Debug;
using BandTogether.Quantum;
using BandTogether.Util;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using UnityEngine;
using static BandTogether.Quantum.QuantumGroup;
using static BandTogether.Quantum.QuantumTarget;

namespace BandTogether;
public class ModMain : ModBehaviour
{
    private static readonly IDictionary<string, (QuantumGroup[] groups, QuantumTarget destination)> GroupDialogueConditions =
        new Dictionary<string, (QuantumGroup[], QuantumTarget)>
        {
            { "NOMAI_VILLAGE_A_TO_DOOR", (new[] { NomaiA }, Door) },
            { "NOMAI_VILLAGE_B_TO_DOOR", (new[] { NomaiB }, Door) },
            { "GHIRD_VILLAGE_A_TO_DOOR", (new[] { GhirdA }, Door) },
            { "GHIRD_VILLAGE_B_TO_DOOR", (new[] { GhirdB }, Door) },

            { "SEARCHED_GREAT_DOOR", (new[] { Captial }, Away) },
            { "CLANS_LEAVE_DOOR", (new[] { NomaiA, NomaiB, GhirdA, GhirdB }, Away) },

            { "DOORKEEPER_TO_FIRE", (new[] { Captial }, Fire) },
            { "NOMAI_TO_FIRE", (new[] { NomaiA, NomaiB }, Fire) },
            { "GHIRD_TO_FIRE", (new[] { GhirdA, GhirdB }, Fire) },
        };

    private static readonly IDictionary<string, QuantumGroup> ShardConditions = new Dictionary<string, QuantumGroup>
    {
        { "GOT_NOMAI_SHARD_A", NomaiA },
        { "GOT_NOMAI_SHARD_B", NomaiB },
        { "GOT_GHIRD_SHARD_A", GhirdA },
        { "GOT_GHIRD_SHARD_B", GhirdB },
    };

    public static ModMain Instance;
    public delegate void MoveNpcEvent(QuantumGroup targetGroup, QuantumTarget targetType, bool ignoreVisibility);
    public event MoveNpcEvent OnMoveGroup;
    public delegate void ModStartEvent();
    public event ModStartEvent OnMainQuest;

    public delegate void ShardFoundEvent(QuantumGroup shardGroup);
    public event ShardFoundEvent OnShardFound;

    public delegate void DialogueConditionChanged(string condition, bool value);
        
    public INewHorizons nhAPI;
    public bool inEndSequence = false;
    public bool fadeEndMusic = false;
    public bool startedEndSequence = false;
    public GameObject Planet { get; private set; }
    
    private bool _debugEnabled = false;
    private int _numClansConvinced;
    private List<GameObject> factsToEnable = new();
    private List<ScrollItem> scrollsToEnable = new();
    private List<GameObject> textToEnable = new();
    private DebugMenu _debugMenu;
    private readonly IDictionary<string, List<DialogueConditionChanged>> _dialogueConditionListeners =
        new Dictionary<string, List<DialogueConditionChanged>>();

    private readonly IDictionary<QuantumGroup, QuantumTarget> _groupCurrentLocation = GroupDialogueConditions
        .Values
        .SelectMany(value => value.groups)
        .Distinct()
        .ToDictionary(key => key, _ => QuantumTarget.Start);

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        nhAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
        nhAPI.LoadConfigs(this);

        if (!EnumUtils.IsDefined<ItemType>("Shrubbery"))
        {
            EnumUtils.Create<ItemType>("Shrubbery", 512);
        }

        LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
        {
            if (loadScene != OWScene.SolarSystem)
            {
                return;
            }

            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);

            nhAPI.GetBodyLoadedEvent().AddListener(OnBodyLoaded);

            GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
            AddDialogueConditionListener(OnMainQuestStart, "MAIN_QUEST_START");
            AddDialogueConditionListener(OnShardCondition, ShardConditions.Keys.ToArray());
            AddDialogueConditionListener(OnGroupMoveCondition, GroupDialogueConditions.Keys.ToArray());

            ModHelper.Events.Unity.FireInNUpdates(() =>
            {
                var relativeLocation = new RelativeLocationData(Vector3.up * 2 + Vector3.forward * 2, Quaternion.identity, Vector3.zero);

                //nessesary for owlks, lets them work somehow????????
                var location = DreamArrivalPoint.Location.Zone3;
                var arrivalPoint = Locator.GetDreamArrivalPoint(location);
                var dreamCampfire = Locator.GetDreamCampfire(location);
                if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() != ItemType.DreamLantern)
                {
                    DreamLanternItem lantern = ReferenceLocator.GetDreamLanternItem();
                    lantern.OnEnterDreamWorld();
                    Locator.GetDreamWorldController()._playerLantern = lantern;
                };

                //Puts player into a semi-dreamworld state
                Locator.GetDreamWorldController().EnterDreamWorld(dreamCampfire, arrivalPoint, relativeLocation);
                Resources.FindObjectsOfTypeAll<GhostBrain>().ToList().ForEach((brain) =>
                {
                    brain.enabled = true;
                    brain.WakeUp();
                    brain.OnEnterDreamWorld();
                });
            }, 15);
        };

        LoadManager.OnStartSceneLoad += (scene, loadScene) =>
        {
            if (scene == OWScene.SolarSystem)
            {
                nhAPI.GetBodyLoadedEvent().RemoveListener(OnBodyLoaded);
                inEndSequence = false;
                fadeEndMusic = false;
                startedEndSequence = false;
                factsToEnable.Clear();
                scrollsToEnable.Clear();
                textToEnable.Clear();
            }
        };
    }

    private void OnBodyLoaded(string bodyName)
    {
        if (bodyName == "Fractured Harmony")
        {
            _debugEnabled = PlayerData.GetPersistentCondition("BAND_TOGETHER_DEBUG_ENABLED");

            Planet = nhAPI.GetPlanet(bodyName);

            if (IsDebugEnabled()) InitDebugMenu();

            ReferenceLocator.GetSacredEntryway().LoadWaterObject(Planet);

            ReferenceLocator.GetCreditsSong().transform.parent = null;

            ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                InitializeConditions();
            });
        }
    }

    public void InitDebugMenu()
    {
        _debugMenu = DebugMenu.InitMenu(Planet);
    } 

    private void InitializeConditions()
    {
        WriteDebugMessage("init conditions");

        if (!GetPersistentCondition("MAIN_QUEST_START"))
        {
            Planet
                .GetComponentsInChildren<ShipLogFactTriggerVolume>()
                .ForEach(factTrigger =>
                {
                    factsToEnable.Add(factTrigger.gameObject);
                    factTrigger.gameObject.SetActive(false);
                });

            Planet.GetComponentsInChildren<ScrollItem>().ForEach(scrollItem =>
            {
                scrollsToEnable.Add(scrollItem);
                scrollItem.EnableInteraction(false);
            });

            Planet.GetComponentsInChildren<NomaiWallText>().ForEach(wallText =>
            {
                textToEnable.Add(wallText.gameObject);
                wallText.gameObject.SetActive(false);
            });
        }

        ShardConditions
            .Where(condition => GetPersistentCondition(condition.Key))
            .ForEach(condition =>
            {
                OnMoveGroup?.Invoke(condition.Value, Door, true);
                OnShardFound?.Invoke(condition.Value);
                _groupCurrentLocation[condition.Value] = Door;
                _numClansConvinced++;
            });

        if (GetPersistentCondition("SHRUB_GIVEN_TO_NOMAI"))
        {
            ShrubberySocketNomai socketNomai = ReferenceLocator.GetShrubSocketNomai();
            socketNomai.PlaceIntoSocket(ReferenceLocator.GetShrubbery());
            socketNomai.EnableInteraction(false);
        }
        else if (GetPersistentCondition("FINISH_SHRUB_QUEST"))
        {
            TheDivineThrone socketThrone = ReferenceLocator.GetShrubSocketThrone();
            socketThrone.PlaceIntoSocket(ReferenceLocator.GetShrubbery());
            if (!GetPersistentCondition("START_STEAL_QUEST"))
            {
                socketThrone.EnableInteraction(false);
            }
        }

        if (GetPersistentCondition("OPEN_SUNPOST_DOOR"))
        {
            ReferenceLocator.GetSunpostDetector().OpenDoor();
        }

        ReferenceLocator.GetLightsOutController().InitializeGhirds();
    }

    private void OnDialogueConditionChanged(string condition, bool value)
    {
        WriteDebugMessage($"condition: {condition}");
        if (_dialogueConditionListeners.TryGetValue(condition, out var listeners))
        {
            listeners.ForEach(listener => listener(condition, value));
        }
    }

    private void OnMainQuestStart(string condition, bool value)
    {
        if (condition != "MAIN_QUEST_START") return;
        
        OnMainQuest?.Invoke();
        if (factsToEnable.Count > 0)
        {
            factsToEnable.ForEach(factTrigger => factTrigger.SetActive(true));
        }
        if (scrollsToEnable.Count > 0)
        {
            scrollsToEnable.ForEach(scrollItem => scrollItem.EnableInteraction(true));
        }
        if (textToEnable.Count > 0)
        {
            textToEnable.ForEach(wallText => wallText.gameObject.SetActive(true));
        }
    }

    private void OnShardCondition(string condition, bool value)
    {
        // Instance.ModHelper.Console.WriteLine($"condition changed: {condition}");
        if (!ShardConditions.TryGetValue(condition, out var shardGroup)) return;
        
        OnShardFound?.Invoke(shardGroup);
            
        _numClansConvinced += 1;
        if (_numClansConvinced == 3 && !GetPersistentCondition("LAST_CLAN_TO_AGREE"))
        {
            SetPersistentCondition("LAST_CLAN_TO_AGREE", true);
        }
        else if (_numClansConvinced == 4 && !GetPersistentCondition("ALL_CLANS_AGREED"))
        {
            Locator.GetShipLogManager().RevealFact("BT_GREAT_DOOR_CLANS_AGREED");
            SetPersistentCondition("ALL_CLANS_AGREED", true);
        }
    }

    private void OnGroupMoveCondition(string condition, bool value)
    {
        if (!GroupDialogueConditions.ContainsKey(condition) || !value) return;

        var destination = GroupDialogueConditions[condition];
        WriteDebugMessage($"move condition: {destination}");

        destination.groups.ForEach(group => OnMoveGroup?.Invoke(group, destination.destination, false));
    }

    public static void AddDialogueConditionListener(DialogueConditionChanged listener, params string[] conditions) =>
        conditions.ForEach(condition =>
            Instance._dialogueConditionListeners
                .GetOrInit(condition, new List<DialogueConditionChanged>())
                .Add(listener)
        );

    public static void RemoveDialogueConditionListener(DialogueConditionChanged listener, params string[] conditions) =>
        conditions.ForEach(condition => 
            Instance._dialogueConditionListeners
                .GetOrInit(condition, new List<DialogueConditionChanged>())
                .Remove(listener)
        );

    public static void SetCondition(string condition, bool value) =>
        DialogueConditionManager.SharedInstance.SetConditionState(condition, value);

    public static bool GetCondition(string condition) =>
        DialogueConditionManager.SharedInstance.GetConditionState(condition);

    public static void SetPersistentCondition(string condition, bool value, bool includeTransient = true)
    {
        PlayerData.SetPersistentCondition(condition, value);
        if(includeTransient) SetCondition(condition, value);
    }

    public static bool GetPersistentCondition(string condition) =>
        PlayerData.GetPersistentCondition(condition);

    public static void TriggerEnd()
    {
        Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
        {
            Instance.startedEndSequence = true;
            Instance.fadeEndMusic = true;
            Locator.GetDeathManager().BeginEscapedTimeLoopSequence(TimeloopEscapeType.Ship);
        });
    }

    public void AudioListenerTest()
    {
        StartCoroutine(ListenerTest());
    }

    private IEnumerator ListenerTest()
    {
        yield return new WaitForSeconds(5f);
        ReferenceLocator.GetCreditsSong().FadeIn(1f, true, false, 1f);
        yield return new WaitForSeconds(5f);
        CenterOfTheUniverse.DeactivateUniverse();
        Locator.GetActiveCamera().enabled = false;
        GameOverController gameOverController = FindObjectOfType<GameOverController>();
        gameOverController._flashbackCamera.enabled = true;
        gameOverController._flashbackCamera.postProcessing.enabled = false;
        gameOverController._audioListener.enabled = true;
    }

    public void OnTriggerCampfireEnd()
    {
        FindObjectOfType<GameOverController>()._deathText.text = "Despite there being nothing of value behind the Great Door, you managed to\nreunite the clans and bring harmony back to the planet.";
        FindObjectOfType<GameOverController>().SetupGameOverScreen(10f);
    }

    public static void WriteDebugMessage(object msg)
    {
        if (!IsDebugEnabled()) return;
        Instance.ModHelper.Console.WriteLine(msg.ToString());
    }

    private void OnDestroy()
    {
        nhAPI.GetBodyLoadedEvent().RemoveListener(OnBodyLoaded);
    }

    public static bool IsDebugEnabled() => Instance._debugEnabled;
}

