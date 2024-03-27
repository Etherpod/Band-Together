using System;
using OWML.Common;
using OWML.ModHelper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BandTogether.QuantumNPC;
using System.Reflection;
using BandTogether.Util;
using HarmonyLib;

namespace BandTogether;
public class ModMain : ModBehaviour
{
    private static readonly IDictionary<string, (GroupType[] groups, GroupDestination destination)> GroupDialogueConditions =
        new Dictionary<string, (GroupType[], GroupDestination)>
        {
            { "NOMAI_VILLAGE_A_TO_DOOR", (new[] { GroupType.NomaiA }, GroupDestination.Door) },
            { "NOMAI_VILLAGE_B_TO_DOOR", (new[] { GroupType.NomaiB }, GroupDestination.Door) },
            { "GHIRD_VILLAGE_A_TO_DOOR", (new[] { GroupType.GhirdA }, GroupDestination.Door) },
            { "GHIRD_VILLAGE_B_TO_DOOR", (new[] { GroupType.GhirdB }, GroupDestination.Door) },

            {
                "CLANS_LEAVE_DOOR",
                (new[] { GroupType.NomaiA, GroupType.NomaiB, GroupType.GhirdA, GroupType.GhirdB }, GroupDestination.Away)
            },
            
            { "NOMAI_TO_FIRE", (new[] { GroupType.NomaiA, GroupType.NomaiB }, GroupDestination.Fire) },
            { "GHIRD_TO_FIRE", (new[] { GroupType.GhirdA, GroupType.GhirdB }, GroupDestination.Fire) },
        };
    
    public static ModMain Instance;
    public delegate void MoveNpcEvent(GroupType target, bool shouldActQuatum);
    public event MoveNpcEvent OnMoveGroup;
    public INewHorizons nhAPI;

    private int _numClansConvinced;
    private List<string> _currentSave = new();
    private GameObject _planet;

    private readonly IDictionary<GroupType, GroupDestination> _groupCurrentLocation = GroupDialogueConditions
        .Values
        .SelectMany(value => value.groups)
        .Distinct()
        .ToDictionary(key => key, _ => GroupDestination.Start);
    private readonly string[] _shardConditions =
    {
        "GOT_NOMAI_SHARD_A",
        "GOT_NOMAI_SHARD_B",
        "GOT_GHIRD_SHARD_A",
        "GOT_GHIRD_SHARD_B"
    };

    private Menu _modMenu;
    private IMenuAPI _menuAPI;
    private Menu _telepoMenu;

    private void Awake()
    {
        Instance = this;
        HarmonyLib.Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        nhAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
        nhAPI.LoadConfigs(this);

        Dictionary<string, List<string>> guy = Instance.ModHelper.Storage.Load<Dictionary<string, List<string>>>("save.json") ?? new();
        var name = StandaloneProfileManager.SharedInstance?.currentProfile?.profileName ?? "xbox";
        _currentSave = guy.ContainsKey(name) ? guy[name] : new List<string>();

        LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
        {
            if (loadScene != OWScene.SolarSystem) return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
            
            nhAPI.GetBodyLoadedEvent().AddListener(OnBodyLoaded);

            GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
                
            ModHelper.Events.Unity.FireInNUpdates(() =>
            {
                var relativeLocation = new RelativeLocationData(Vector3.up * 2 + Vector3.forward * 2, Quaternion.identity, Vector3.zero);

                //nessesary for owlks, lets them work somehow????????
                var location = DreamArrivalPoint.Location.Zone3;
                var arrivalPoint = Locator.GetDreamArrivalPoint(location);
                var dreamCampfire = Locator.GetDreamCampfire(location);
                if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() != ItemType.DreamLantern)
                {
                    var dreamLanternItem = GameObject.Find("THE_GOD_OF_ALL_LANTERNS_PIKPIK_CARROT").GetComponent<DreamLanternItem>();
                    dreamLanternItem.OnEnterDreamWorld();
                    Locator.GetDreamWorldController()._playerLantern = dreamLanternItem;
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

            ModHelper.Events.Unity.FireInNUpdates(CreateDebugMenus, 25);
        };
    }

    private void CreateDebugMenus()
    {
        _menuAPI = ModHelper.Interaction.TryGetModApi<IMenuAPI>("_nebula.MenuFramework");
        _modMenu = _menuAPI.PauseMenu_MakePauseListMenu("BAND TOGETHER DEBUG ACTIONS");
        _menuAPI.PauseMenu_MakeMenuOpenButton("BAND-TOGETHER DEBUG ACTIONS", _modMenu);

        _telepoMenu = _menuAPI.PauseMenu_MakePauseListMenu("TELEPORT DESTINATIONS");
        _menuAPI.PauseMenu_MakeMenuOpenButton("TELEPORT TO PLANET", _telepoMenu, _modMenu);
        AddTeleportButton("NORTH POLE", "NorthPole");
        AddTeleportButton("SOUTH POLE", "SouthPole");
        AddTeleportButton("THE DOOR", "TheDoor");
        AddTeleportButton("NOMAI // COCKPIT", "NomaiCockpit");
        AddTeleportButton("NOMAI // OTHER", "NomaiOther");
        AddTeleportButton("BIRB // FOLLOWERS OF ITS GRAND EPHEMERAL ARBOREAL ILLUMINATING ETERNAL SOVEREIGN CELESTIAL TRANQUIL BEARER, THE SACRED SHRUBBERY", "GhirdShrubbery");
        AddTeleportButton("BIRB // LOGIC", "GhirdLogic");
    }

    private void AddTeleportButton(string buttonText, string targetName)
    {
        _menuAPI.PauseMenu_MakeSimpleButton(buttonText, _telepoMenu).onClick.AddListener(() =>
        {
            TeleportPlayer(targetName);
        });
    }

    private void TeleportPlayer(string target)
    {
        var playerBody = Locator.GetPlayerBody();
        var destination = _planet.transform.Find($"Sector/JamPlanet/Debug/TeleportDestinations/{target}");
        var planetBody = _planet.GetComponent<OWRigidbody>();
        playerBody.SetPosition(destination.position + 2*(destination.rotation*Vector3.up));
        playerBody.SetRotation(destination.rotation);
        playerBody.SetVelocity(planetBody.GetVelocity());
    }

    private void OnBodyLoaded(string bodyName)
    {
        if (bodyName == "Fractured Harmony")
        {
            _planet = nhAPI.GetPlanet(bodyName);
            _planet
                .transform
                .Find("Sector/JamPlanet/GhirdCityB/CityHall/house/SacredEntrywayTrigger")
                .GetComponent<SacredEntrywayTrigger>()
                .LoadWaterObject(_planet);

            InitializeConditions();
        }
    }

    private void InitializeConditions()
    {
        SetSavedCondition("test", true);
        WriteMessage("Count: " + _currentSave.Count);
        if (GetSavedConditionList() != null)
        {
            foreach (string savedCondition in GetSavedConditionList())
            {
                WriteMessage(savedCondition);
                DialogueConditionManager.SharedInstance.SetConditionState(savedCondition.Split(':').First(), savedCondition.EndsWith("true"));
            }
        }
        
        if (CheckCondition("GOT_NOMAI_SHARD_A"))
        {
            OnMoveGroup?.Invoke(GroupType.NomaiA, false);
            _groupCurrentLocation[GroupType.NomaiA] = GroupDestination.Door;
            _numClansConvinced++;
        }
        if (CheckCondition("GOT_NOMAI_SHARD_B"))
        {
            OnMoveGroup?.Invoke(GroupType.NomaiB, false);
            _groupCurrentLocation[GroupType.NomaiB] = GroupDestination.Door;
            _numClansConvinced++;
        }
        if (CheckCondition("GOT_GHIRD_SHARD_A"))
        {
            OnMoveGroup?.Invoke(GroupType.GhirdA, false);
            _groupCurrentLocation[GroupType.GhirdA] = GroupDestination.Door;
            _numClansConvinced++;
        }
        if (CheckCondition("GOT_GHIRD_SHARD_B"))
        {
            OnMoveGroup?.Invoke(GroupType.GhirdB, false);
            _groupCurrentLocation[GroupType.GhirdB] = GroupDestination.Door;
            _numClansConvinced++;
        }

        if (CheckCondition("START_STEAL_QUEST"))
        {
            FindObjectOfType<TheDivineThrone>().PlaceIntoSocket(FindObjectOfType<Shrubbery>());
        }

        /*if (_numClansConvinced == 3 && !CheckCondition("LAST_CLAN_TO_AGREE"))
        {
            SetSavedCondition("LAST_CLAN_TO_AGREE", true);
        }
        else if (_numClansConvinced == 4 && !CheckCondition("ALL_CLANS_AGREED"))
        {
            SetSavedCondition("ALL_CLANS_AGREED", true);
        }*/
    }

    public static bool CheckCondition(string condition)
    {
        return DialogueConditionManager.SharedInstance.ConditionExists(condition) && DialogueConditionManager.SharedInstance._dictConditions[condition];
    }

    private void OnDialogueConditionChanged(string condition, bool value)
    {
        Instance.ModHelper.Console.WriteLine($"condition changed: {condition}");
        if (_shardConditions.Contains(condition))
        {
            _numClansConvinced += 1;
            if (_numClansConvinced == 3 && !CheckCondition("LAST_CLAN_TO_AGREE"))
            {
                SetSavedCondition("LAST_CLAN_TO_AGREE", true);
            }
            else if (_numClansConvinced == 4 && !CheckCondition("ALL_CLANS_AGREED"))
            {
                SetSavedCondition("ALL_CLANS_AGREED", true);
                Locator.GetShipLogManager().RevealFact("GREAT_DOOR_CLANS_AGREED");
            }
        }

        if (!GroupDialogueConditions.ContainsKey(condition) || !value) return;

        var destination = GroupDialogueConditions[condition];
        var groupsToMove = destination
            .groups
            .Where(group => _groupCurrentLocation[group] < destination.destination)
            .ToList();
        Instance.ModHelper.Console.WriteLine($"groupsToMove: {groupsToMove.Count}");
        if (groupsToMove.Count() is 0) return;
        
        foreach (var group in groupsToMove)
        {
            Instance.ModHelper.Console.WriteLine($"moving [{group}] to: {destination.destination}");

            OnMoveGroup?.Invoke(group, true);
            _groupCurrentLocation[group] = destination.destination;
        }
    }

    public static void SetSavedCondition(string condition, bool value)
    {
        //Loads the save or makes a new one
        Dictionary<string, List<string>> guy = Instance.ModHelper.Storage.Load<Dictionary<string, List<string>>>("save.json") ?? new();
        //Gets the name to use for the save dict
        var name = StandaloneProfileManager.SharedInstance?.currentProfile?.profileName ?? "xbox";
        //Gets the save data for the current profile
        List<string> save = guy.ContainsKey(name) ? guy[name] : new List<string>();

        List<string> toRemove = new();

        save.RemoveAll(savedCondition => savedCondition.StartsWith(condition + ":"));

        //Re-adds condition
        save.Add(condition + ":" + value);
        guy[name] = save;
        Instance.ModHelper.Storage.Save(guy, "save.json");
        Instance._currentSave = save;

        DialogueConditionManager.SharedInstance.SetConditionState(condition, value);
    }

    public static bool GetSavedCondition(string condition)
    {
        if (Instance._currentSave.Count == 0) { return false; }
        string savedCondition = Instance._currentSave.Where(savedCondition => savedCondition.StartsWith(condition + ":")).First();
        return savedCondition.EndsWith("true");
    }

    public static List<string> GetSavedConditionList()
    {
        if (Instance._currentSave.Count == 0) return null;
        return Instance._currentSave;
    }

    public static void WriteMessage(string msg)
    {
        Instance.ModHelper.Console.WriteLine(msg);
    }

    private enum GroupDestination
    {
        Start,
        Door,
        Away,
        Fire,
    }
}

