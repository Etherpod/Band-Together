using System;
using OWML.Common;
using OWML.ModHelper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BandTogether.QuantumNPC;
using System.Reflection;
using HarmonyLib;

namespace BandTogether;
public class ModMain : ModBehaviour
{
    private static readonly IDictionary<string, (GroupType[] groups, GroupDestination destination)> GroupDialogueConditions =
        new Dictionary<string, (GroupType[], GroupDestination)>
        {
            { "NOMAI_VILLAGE_A_TO_DOOR", (new[] { GroupType.NomaiA }, GroupDestination.Door) },
            { "NOMAI_VILLAGE_A_TO_FIRE", (new[] { GroupType.NomaiA }, GroupDestination.Fire) },

            { "NOMAI_VILLAGE_B_TO_DOOR", (new[] { GroupType.NomaiB }, GroupDestination.Door) },
            { "NOMAI_VILLAGE_B_TO_FIRE", (new[] { GroupType.NomaiB }, GroupDestination.Fire) },

            { "GHIRD_VILLAGE_A_TO_DOOR", (new[] { GroupType.GhirdA }, GroupDestination.Door) },
            { "GHIRD_VILLAGE_A_TO_FIRE", (new[] { GroupType.GhirdA }, GroupDestination.Fire) },

            { "GHIRD_VILLAGE_B_TO_DOOR", (new[] { GroupType.GhirdB }, GroupDestination.Door) },
            { "GHIRD_VILLAGE_B_TO_FIRE", (new[] { GroupType.GhirdB }, GroupDestination.Fire) },

            {
                "CLANS_LEAVE_DOOR",
                (new[] { GroupType.NomaiA, GroupType.NomaiB, GroupType.GhirdA, GroupType.GhirdB }, GroupDestination.Away)
            },
        };
    
    public static ModMain Instance;
    public delegate void MoveNpcEvent(GroupType target, bool shouldActQuatum);
    public event MoveNpcEvent OnMoveGroup;
    public INewHorizons nhAPI;

    private int numClansConvinced;

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

    private void Awake()
    {
        Instance = this;
        HarmonyLib.Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        // Starting here, you'll have access to OWML's mod helper.
        //ModHelper.Console.WriteLine($"My mod {nameof(ModMain)} is loaded!", MessageType.Success);

        // Get the New Horizons API and load configs
        nhAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
        nhAPI.LoadConfigs(this);

        // Example of accessing game code.
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

        };
    }

    private void OnBodyLoaded(string bodyName)
    {
        if (bodyName == "Fractured Harmony")
        {
            var planet = nhAPI.GetPlanet(bodyName);
            planet
                .transform
                .Find("Sector/JamPlanet/GhirdCityB/CityHall/house/SacredEntrywayTrigger")
                .GetComponent<SacredEntrywayTrigger>()
                .LoadWaterObject(planet);

            MoveGroupsToDoor();
        }
    }

    private void MoveGroupsToDoor()
    {
        if (CheckCondition("GOT_NOMAI_SHARD_A"))
        {
            OnMoveGroup?.Invoke(GroupType.NomaiA, false);
            _groupCurrentLocation[GroupType.NomaiA] = GroupDestination.Door;
            numClansConvinced++;
        }
        if (CheckCondition("GOT_NOMAI_SHARD_B"))
        {
            OnMoveGroup?.Invoke(GroupType.NomaiB, false);
            _groupCurrentLocation[GroupType.NomaiB] = GroupDestination.Door;
            numClansConvinced++;
        }
        if (CheckCondition("GOT_GHIRD_SHARD_A"))
        {
            OnMoveGroup?.Invoke(GroupType.GhirdA, false);
            _groupCurrentLocation[GroupType.GhirdA] = GroupDestination.Door;
            numClansConvinced++;
        }
        if (CheckCondition("GOT_GHIRD_SHARD_B"))
        {
            OnMoveGroup?.Invoke(GroupType.GhirdB, false);
            _groupCurrentLocation[GroupType.GhirdB] = GroupDestination.Door;
            numClansConvinced++;
        }

        if (numClansConvinced == 3)
        {
            DialogueConditionManager.SharedInstance.SetConditionState("LAST_CLAN_TO_AGREE", true);
        }
        else if (numClansConvinced == 4)
        {
            DialogueConditionManager.SharedInstance.SetConditionState("ALL_CLANS_AGREED", true);
        }
    }

    private bool CheckCondition(string condition)
    {
        return DialogueConditionManager.SharedInstance.ConditionExists(condition) && DialogueConditionManager.SharedInstance._dictConditions[condition];
    }

    private void OnDialogueConditionChanged(string condition, bool value)
    {
        // Instance.ModHelper.Console.WriteLine($"condition changed: {condition}");
        if (_shardConditions.Contains(condition))
        {
            numClansConvinced += 1;
            if (numClansConvinced == 3)
            {
                DialogueConditionManager.SharedInstance.SetConditionState("LAST_CLAN_TO_AGREE", true);
            }
            else if (numClansConvinced == 4)
            {
                DialogueConditionManager.SharedInstance.SetConditionState("ALL_CLANS_AGREED", true);
            }
        }

        if (!GroupDialogueConditions.ContainsKey(condition) || !value) return;

        var destination = GroupDialogueConditions[condition];
        var groupsToMove = destination
            .groups
            .Where(group => _groupCurrentLocation[group] < destination.destination)
            .ToList();
        // Instance.ModHelper.Console.WriteLine($"groupsToMove: {groupsToMove.Join()}");
        if (groupsToMove.Count() is 0) return;
        
        foreach (var group in groupsToMove)
        {
            // Instance.ModHelper.Console.WriteLine($"moving [{group}] to: {destination.destination.ToString()}");

            OnMoveGroup?.Invoke(group, true);
            _groupCurrentLocation[group] = destination.destination;
        }
    }

    private enum GroupDestination
    {
        Start,
        Door,
        Away,
        Fire,
    }
}

