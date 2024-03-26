using System;
using OWML.Common;
using OWML.ModHelper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BandTogether.QuantumNPC;
using System.Reflection;

namespace BandTogether;
public class ModMain : ModBehaviour
{
    private static readonly IDictionary<string, (GroupType group, GroupDestination destination)> GroupDialogueConditions =
        new Dictionary<string, (GroupType, GroupDestination)>
        {
            { "NOMAI_VILLAGE_A_TO_DOOR", (GroupType.NomaiA, GroupDestination.Door) },
            { "NOMAI_VILLAGE_A_TO_AWAY", (GroupType.NomaiA, GroupDestination.Away) },
            { "NOMAI_VILLAGE_A_TO_FIRE", (GroupType.NomaiA, GroupDestination.Fire) },
            
            { "NOMAI_VILLAGE_B_TO_DOOR", (GroupType.NomaiB, GroupDestination.Door) },
            { "NOMAI_VILLAGE_B_TO_AWAY", (GroupType.NomaiB, GroupDestination.Away) },
            { "NOMAI_VILLAGE_B_TO_FIRE", (GroupType.NomaiB, GroupDestination.Fire) },
            
            { "GHIRD_VILLAGE_A_TO_DOOR", (GroupType.GhirdA, GroupDestination.Door) },
            { "GHIRD_VILLAGE_A_TO_AWAY", (GroupType.GhirdA, GroupDestination.Away) },
            { "GHIRD_VILLAGE_A_TO_FIRE", (GroupType.GhirdA, GroupDestination.Fire) },
            
            { "GHIRD_VILLAGE_B_TO_DOOR", (GroupType.GhirdB, GroupDestination.Door) },
            { "GHIRD_VILLAGE_B_TO_AWAY", (GroupType.GhirdB, GroupDestination.Away) },
            { "GHIRD_VILLAGE_B_TO_FIRE", (GroupType.GhirdB, GroupDestination.Fire) },
        };
    
    public static ModMain Instance;
    public delegate void MoveNpcEvent(GroupType target, bool shouldActQuatum);
    public event MoveNpcEvent OnMoveGroup;
    public INewHorizons nhAPI;

    private readonly IDictionary<GroupType, GroupDestination> _groupCurrentLocation = GroupDialogueConditions
        .Values
        .Select(value => value.group)
        .Distinct()
        .ToDictionary(key => key, _ => GroupDestination.Start);

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
        }
        if (CheckCondition("GOT_NOMAI_SHARD_B"))
        {
            OnMoveGroup?.Invoke(GroupType.NomaiA, false);
            _groupCurrentLocation[GroupType.NomaiB] = GroupDestination.Door;
        }
        if (CheckCondition("GOT_GHIRD_SHARD_A"))
        {
            OnMoveGroup?.Invoke(GroupType.NomaiA, false);
            _groupCurrentLocation[GroupType.GhirdA] = GroupDestination.Door;
        }
        if (CheckCondition("GOT_GHIRD_SHARD_B"))
        {
            OnMoveGroup?.Invoke(GroupType.NomaiA, false);
            _groupCurrentLocation[GroupType.GhirdB] = GroupDestination.Door;
        }
    }

    private bool CheckCondition(string condition)
    {
        return DialogueConditionManager.SharedInstance.ConditionExists(condition) && DialogueConditionManager.SharedInstance._dictConditions[condition];
    }

    private void OnDialogueConditionChanged(string condition, bool value)
    {
        if (!GroupDialogueConditions.ContainsKey(condition) || !value) return;

        var destination = GroupDialogueConditions[condition];
        if (destination.destination <= _groupCurrentLocation[destination.group]) return;
        
        OnMoveGroup?.Invoke(destination.group, true);
        _groupCurrentLocation[destination.group] = destination.destination;
    }

    private enum GroupDestination
    {
        Start,
        Door,
        Away,
        Fire,
    }
}

