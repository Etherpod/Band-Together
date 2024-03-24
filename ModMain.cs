using OWML.Common;
using OWML.ModHelper;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BandTogether;
public class ModMain : ModBehaviour
{
    public static ModMain Instance;
    public delegate void MoveVillageEvent(string target);
    public event MoveVillageEvent OnMoveVillage;
    public INewHorizons nhAPI;

    bool movedToDoor = false;

    private void Awake()
    {
        Instance = this;
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

    private void Update()
    {
        if (!movedToDoor && DialogueConditionManager.SharedInstance.ConditionExists("VILLAGE_B_TO_DOOR") && DialogueConditionManager.SharedInstance._dictConditions["VILLAGE_B_TO_DOOR"])
        {
            movedToDoor = true;
            if (OnMoveVillage == null) { return; }
            ModHelper.Console.WriteLine("Ok did event");
            OnMoveVillage("NOMAI_B");
        }
    }
}

