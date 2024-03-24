using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandTogether
{
    internal class MyPatchClass
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
}
