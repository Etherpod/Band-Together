using OWML.Common;
using OWML.ModHelper;
using System.Collections;
using UnityEngine;

namespace BandTogether;
public class ModMain : ModBehaviour
{
    public static ModMain Instance;
    public delegate void MoveVillageEvent(string target);
    public event MoveVillageEvent OnMoveVillage;

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
        var newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
        newHorizons.LoadConfigs(this);

        // Example of accessing game code.
        LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
        {
            if (loadScene != OWScene.SolarSystem) return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
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

