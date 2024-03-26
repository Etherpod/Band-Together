using HarmonyLib;

namespace BandTogether;
[HarmonyPatch]
public class MyPatchClass
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostSensors), nameof(GhostSensors.FixedUpdate_Sensors))]
    public static void FlashLightOwlk(GhostSensors __instance)
    {
        __instance._data.sensor.isIlluminatedByPlayer = __instance._data.sensor.isIlluminated;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnSnapPlayerNeck))]
    public static void OwlkSnap(GhostGrabController __instance)
    {
        if (!Locator.GetDeathManager().IsPlayerDying() && !Locator.GetDeathManager().IsPlayerDead())
        {
            Locator.GetDeathManager().KillPlayer(DeathType.CrushedByElevator);
        }
    }
}
