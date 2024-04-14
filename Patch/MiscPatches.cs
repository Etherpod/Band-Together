using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace BandTogether.Patch;

[HarmonyPatch]
public class MiscPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostSensors), nameof(GhostSensors.FixedUpdate_Sensors))]
    public static void FlashLightOwlk(GhostSensors __instance)
    {
        __instance._data.sensor.isIlluminatedByPlayer = __instance._data.sensor.isIlluminated;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GhostGrabController), nameof(GhostGrabController.OnSnapPlayerNeck))]
    public static bool GhostGrabController_OnSnapPlayerNeck_Prefix(GhostGrabController __instance)
    {
        if (!Locator.GetDeathManager().IsPlayerDying() && !Locator.GetDeathManager().IsPlayerDead())
        {
            Locator.GetDreamWorldController().ExitDreamWorld(DreamWakeType.NeckSnapped);
            RespawnTeleport();
            __instance.ReleasePlayer();
            GhostBrain brain = __instance.GetComponentInParent<GhostBrain>();
            brain.ChangeAction(null);
            brain._data.OnPlayerExitDreamWorld();
            ReticleController.Show();
            Locator.GetPromptManager().SetPromptsVisible(true);
            Locator.GetDreamWorldController()._playerCamEffectController.OpenEyes(0.5f, false);
        }

        __instance.enabled = false;

        return false;
    }

    private static void RespawnTeleport()
    {
        var playerBody = Locator.GetPlayerBody();
        var destination = ReferenceLocator.GetPlayerRespawnPoint();
        var planetBody = ModMain.Instance.Planet.GetComponent<OWRigidbody>();

        var targetRotation = destination.rotation;
        var targetPosition = destination.position + 2 * (targetRotation * Vector3.up);
        var targetVelocity = planetBody.GetVelocity();

        playerBody.SetPosition(targetPosition);
        playerBody.SetRotation(targetRotation);
        playerBody.SetVelocity(targetVelocity);

        TheDivineThrone throneSocket = ReferenceLocator.GetShrubSocketThrone();
        ItemTool itemTool = UnityEngine.Object.FindObjectOfType<ItemTool>();
        Shrubbery shrub = ReferenceLocator.GetShrubbery();

        if (itemTool.GetHeldItemType() == shrub.GetItemType())
        {
            itemTool.SocketItem(throneSocket);
            ModMain.SetCondition("HAS_SHRUBBERY", false);
        }
        else if (!throneSocket.IsSocketOccupied())
        {
            throneSocket.PlaceIntoSocket(shrub);
        }

        shrub.transform.localScale = Vector3.one;
        throneSocket.EnableInteraction(true);
        ReferenceLocator.GetShrubSocketNomai().EnableInteraction(false);

        SacredEntrywayTrigger entryway = ReferenceLocator.GetSacredEntryway();
        entryway.ForceSetEnabled(true);
        //ReferenceLocator.GetGhirdVillageBDarkZone().OnExit(Locator.GetPlayerDetector());

        ReferenceLocator.GetFlashlightRuleset().OnExit();

        CageElevator elevator = ReferenceLocator.GetGhirdVillageBElevator();
        elevator._currentDestinationIdx = elevator._destinations.Length - 1;
        elevator._ghostInterface.SetStartingPosition(true);
        elevator.SetReached(true, true, true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DialogueNode), nameof(DialogueNode.EntryConditionsSatisfied))]
    public static void DialogueEntryConditionsSatisfied(DialogueNode __instance, ref bool __result)
    {
        // ModMain.WriteMessage("entry condition patch");

        if (__instance._listEntryCondition.Count == 0)
        {
            // ModMain.WriteMessage("no entry conditions");
            __result = false;
            return;
        }

        ;

        var sharedInstance = DialogueConditionManager.SharedInstance;
        __result = __instance._listEntryCondition
            .All(condition =>
            {
                // ModMain.WriteMessage($"checking condition: {condition}");

                if (PlayerData.PersistentConditionExists(condition))
                {
                    // ModMain.WriteMessage($"found persistent condition value: {PlayerData.GetPersistentCondition(condition)}");
                    return PlayerData.GetPersistentCondition(condition);
                }

                if (sharedInstance.ConditionExists(condition))
                {
                    // ModMain.WriteMessage($"found condition value: {sharedInstance.GetConditionState(condition)}");
                    return sharedInstance.GetConditionState(condition);
                }

                // ModMain.WriteMessage("condition not found");
                return false;
            });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DeathManager), nameof(DeathManager.FinishEscapeTimeLoopSequence))]
    public static void CallCampfireEnd(DeathManager __instance)
    {
        if (!__instance._escapedTimeLoopSequenceComplete && !ModMain.Instance.inEndSequence)
        {
            __instance._escapedTimeLoopSequenceComplete = true;
            ModMain.Instance.inEndSequence = true;
            ModMain.Instance.OnTriggerCampfireEnd();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.Update))]
    public static bool AudioListenerVolumeUndo(PlayerCameraEffectController __instance)
    {
        if (__instance._waitForWakeInput && LateInitializerManager.isDoneInitializing)
        {
            if (!__instance._wakePrompt.IsVisible())
            {
                __instance._wakePrompt.SetVisibility(true);
            }
            if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.All))
            {
                __instance._waitForWakeInput = false;
                LateInitializerManager.pauseOnInitialization = false;
                Locator.GetPauseCommandListener().RemovePauseCommandLock();
                Locator.GetPromptManager().RemoveScreenPrompt(__instance._wakePrompt);
                OWTime.Unpause(OWTime.PauseType.Sleeping);
                __instance.WakeUp();
            }
        }
        if (__instance._playerResources.GetHazardDetector().GetNetDamagePerSecond() > 0f)
        {
            __instance.ApplyExposureDamage();
        }
        if (__instance._isDying)
        {
            float num = Mathf.InverseLerp(__instance._deathStartTime, __instance._deathStartTime + __instance._deathFadeLength, Time.time);
            switch (__instance._deathType)
            {
                case DeathType.Default:
                case DeathType.Impact:
                case DeathType.Crushed:
                case DeathType.Dream:
                    if (__instance._impactDeathSpeed < __instance._impactQuickDeathSpeed)
                    {
                        __instance._owCamera.postProcessingSettings.eyeMask.openness = 1f - __instance._impactSlowCurve.Evaluate(num);
                        __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, -5f, __instance._impactSlowCurve.Evaluate(num));
                    }
                    else
                    {
                        __instance._owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
                        __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num);
                    }
                    break;
                case DeathType.Asphyxiation:
                    {
                        float num2 = (2f - num) * num;
                        __instance._owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
                        __instance._owCamera.postProcessingSettings.bloom.threshold = Mathf.Lerp(__instance._owCamera.postProcessingSettings.bloomDefault.threshold, 0f, num2);
                        __instance._owCamera.postProcessingSettings.bloom.radius = Mathf.Lerp(__instance._owCamera.postProcessingSettings.bloomDefault.radius, 7f, num2);
                        __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num2);
                        __instance._owCamera.postProcessingSettings.colorGrading.saturation = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.saturation, 0f, num2);
                        break;
                    }
                case DeathType.Energy:
                case DeathType.Supernova:
                case DeathType.Lava:
                case DeathType.DreamExplosion:
                    __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num);
                    break;
                case DeathType.Digestion:
                    __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num);
                    break;
                case DeathType.BigBang:
                    {
                        float num3 = Mathf.Clamp01((Time.time - __instance._deathStartTime) / __instance._energyFadeLength);
                        __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num3);
                        break;
                    }
                case DeathType.Meditation:
                    __instance._owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
                    __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num);
                    break;
                case DeathType.TimeLoop:
                    __instance._owCamera.postProcessingSettings.eyeMask.openness = 1f - __instance._timeLoopEyeMaskCurve.Evaluate(num);
                    __instance._owCamera.postProcessingSettings.eyeMask.linesProgress = __instance._timeLoopLinesProgressionCurve.Evaluate(num);
                    break;
                case DeathType.BlackHole:
                    __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num);
                    break;
                case DeathType.CrushedByElevator:
                    __instance._owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
                    __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num);
                    break;
                default:
                    __instance._owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
                    __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num);
                    break;
            }
            if (num >= 1f && !__instance._deathSequenceFinished)
            {
                __instance._deathSequenceFinished = true;
                if (Locator.GetDeathManager().IsFakeMeditationDeath())
                {
                    __instance.ResetDeathSequence();
                    __instance._owCamera.postProcessingSettings.eyeMask.openness = 0f;
                }
                Locator.GetDeathManager().FinishDeathSequence();
            }
        }
        else if (__instance._isOpeningEyes)
        {
            float num4 = Mathf.Clamp01((Time.time - __instance._eyeAnimStartTime) / __instance._eyeAnimDuration);
            float num5 = __instance._wakeCurve.Evaluate(num4);
            __instance._owCamera.postProcessingSettings.eyeMask.openness = num5;
            __instance._owCamera.postProcessingSettings.bloom.threshold = Mathf.Lerp(0f, __instance._owCamera.postProcessingSettings.bloomDefault.threshold, num5);
            if (__instance._lastOpenness >= 0.5f && num5 < 0.5f)
            {
                GlobalMessenger.FireEvent("PlayerBlink");
            }
            __instance._lastOpenness = num5;
            if (num4 >= 1f)
            {
                __instance._owCamera.postProcessingSettings.eyeMaskEnabled = false;
                __instance._isOpeningEyes = false;
                GlobalMessenger.FireEvent("FinishOpenEyes");
            }
        }
        else if (__instance._isClosingEyes)
        {
            float num6 = Mathf.Clamp01((Time.time - __instance._eyeAnimStartTime) / __instance._eyeAnimDuration);
            __instance._owCamera.postProcessingSettings.eyeMask.openness = 1f - num6;
            __instance._owCamera.postProcessingSettings.bloom.threshold = Mathf.Lerp(0f, __instance._owCamera.postProcessingSettings.bloomDefault.threshold, 1f - num6);
            if (num6 >= 1f)
            {
                __instance._isClosingEyes = false;
            }
        }
        else if (__instance._isWincing)
        {
            float num7 = Mathf.Clamp01((Time.time - __instance._winceStartTime) / __instance._winceFadeLength);
            float num8 = 1f - __instance._winceEffectCurve.Evaluate(num7);
            __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(__instance._winceExposure.Evaluate(__instance._winceDamage), __instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, num8);
            __instance._owCamera.postProcessingSettings.colorGrading.saturation = Mathf.Lerp(__instance._winceSaturation.Evaluate(__instance._winceDamage), __instance._owCamera.postProcessingSettings.colorGradingDefault.saturation, num8);
            __instance._owCamera.postProcessingSettings.colorGrading.contrast = Mathf.Lerp(__instance._winceContrast.Evaluate(__instance._winceDamage), __instance._owCamera.postProcessingSettings.colorGradingDefault.contrast, num8);
            if (num7 >= 1f)
            {
                __instance._isWincing = false;
                __instance._winceDamage = 0f;
            }
        }
        if (__instance._isConsciousnessFading)
        {
            float num9 = Mathf.Clamp01((Time.time - __instance._consciousnessFadeStartTime) / __instance._consciousnessFadeLength);
            __instance._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Min(Mathf.Lerp(__instance._owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num9), __instance._owCamera.postProcessingSettings.colorGrading.postExposure);

            if (ModMain.Instance.startedEndSequence)
            {
                ModMain.WriteDebugMessage(AudioListener.volume);
                if (ModMain.Instance.fadeEndMusic)
                {
                    ModMain.Instance.fadeEndMusic = false;
                    Locator.GetAudioMixer().MixSleepAtCampfire(5f);
                }
            }
            else
            {
                AudioListener.volume = 1f - num9;
            }

            if (num9 >= 1f)
            {
                Locator.GetDeathManager().FinishEscapeTimeLoopSequence();
            }
        }
        if (__instance._isRealityShattering && !__instance._isRealityShatterEffectComplete)
        {
            float num10 = Time.time - __instance._realityShatterStartTime;
            __instance._realityShatterEffect.SetShatterParameters(__instance._realityShardShatterCurve.Evaluate(num10), __instance._realityShardOffsetCurve.Evaluate(num10), __instance._realityShardDissolveWidthCurve.Evaluate(num10), __instance._realityShardDissolveProgressCurve.Evaluate(num10));
            if (num10 >= __instance._realityShatterLength)
            {
                __instance._isRealityShatterEffectComplete = true;
                if (typeof(PlayerCameraEffectController).GetField("OnRealityShatterEffectComplete")?.GetValue(__instance) is MulticastDelegate multiDelegate)
                {
                    multiDelegate.DynamicInvoke();
                }
            }
        }
        if (__instance._owCamera.postProcessingSettings.phosphenesEnabled)
        {
            float num11 = Mathf.Clamp01((Time.time - __instance._winceStartTime) / __instance._phospheneFadeLength);
            __instance._owCamera.postProcessingSettings.phosphenes.visibility = 1f - num11;
            __instance._owCamera.postProcessingSettings.phosphenes.brightness = __instance._phospheneBrightness.Evaluate(num11);
            if (num11 >= 1f)
            {
                __instance._owCamera.postProcessingSettings.phosphenesEnabled = false;
            }
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(QuantumObject), nameof(QuantumObject.IsLockedByPlayerContact))]
    public static void LockedByContactOverride(QuantumObject __instance, ref bool __result)
    {
        if (__instance.GetComponent<MapModeQuantumObject>())
        {
            __result = __instance.IsPlayerEntangled() && !Locator.GetMapController()._isMapMode;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SocketedQuantumObject), nameof(SocketedQuantumObject.MoveToSocket))]
    public static void MoveToSocketPostfix(SocketedQuantumObject __instance)
    {
        if (__instance.IsPlayerEntangled() && __instance.TryGetComponent(out MapModeQuantumObject quantumObj) && quantumObj.ernestoRock)
        {
            quantumObj.OnTeleport();
        }
    }
}