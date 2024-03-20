using OWML.Common;
using OWML.ModHelper;

namespace OWModJam;
public class ModMain : ModBehaviour
{
    public static ModMain Instance;

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
}

