using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using QSB;
using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace BandTogether.Patch;

[HarmonyPatch]
public class MiscPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GhostSensors), nameof(GhostSensors.FixedUpdate_Sensors))]
    public static void FlashLightOwlk(GhostSensors __instance)
    {
        if (ModMain.qsbEnabled) { return; }

        __instance._data.sensor.isIlluminatedByPlayer = __instance._data.sensor.isIlluminated;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(QSBGhostSensors), nameof(QSBGhostSensors.FixedUpdate_Sensors))]
    public static void FlashLightOwlkQSB(QSBGhostSensors __instance)
    {
        /*if (!ModMain.qsbEnabled) { return; }

        foreach (var pair in __instance._data.players)
        {
            var player = pair.Value;

            var lanternController = player.player.AssignedSimulationLantern.AttachedObject.GetLanternController();
            player.sensor.isPlayerHoldingLantern = true;
            player.sensor.isPlayerHeldLanternVisible = !lanternController.IsConcealed();
            player.sensor.isPlayerDroppedLanternVisible = false;
            player.sensor.isIlluminatedByPlayer = __instance.AttachedObject._lightSensor.IsIlluminatedByLantern(lanternController);

            if (!lanternController.IsConcealed() || player.player.LightSensor.IsIlluminated())
            {
                var position = 
            }
        }*/
        if (__instance._data == null)
        {
            return;
        }

        foreach (var pair in __instance._data.players)
        {
            var player = pair.Value;

            if (player.player.AssignedSimulationLantern == null)
            {
                continue;
            }

            var lanternController = player.player.AssignedSimulationLantern.AttachedObject.GetLanternController();
            var playerLightSensor = player.player.LightSensor;
            player.sensor.isPlayerHoldingLantern = true;
            __instance._data.isIlluminated = __instance.AttachedObject._lightSensor.IsIlluminated();
            //player.sensor.isIlluminatedByPlayer = (lanternController.IsHeldByPlayer() && __instance.AttachedObject._lightSensor.IsIlluminatedByLantern(lanternController));
            //player.sensor.isIlluminatedByPlayer = __instance.AttachedObject._lightSensor.IsIlluminatedByLantern(lanternController);
            player.sensor.isIlluminatedByPlayer = __instance._data.isIlluminated;
            player.sensor.isPlayerIlluminatedByUs = playerLightSensor.IsIlluminatedByLantern(__instance.AttachedObject._lantern);
            //player.sensor.isPlayerIlluminated = playerLightSensor.IsIlluminated();
            player.sensor.isPlayerIlluminated = playerLightSensor.IsIlluminated() || Locator.GetFlashlight().IsFlashlightOn();
            player.sensor.isPlayerVisible = false;
            player.sensor.isPlayerHeldLanternVisible = false;
            player.sensor.isPlayerDroppedLanternVisible = false;
            player.sensor.isPlayerOccluded = false;

            //if ((lanternController.IsHeldByPlayer() && !lanternController.IsConcealed()) || playerLightSensor.IsIlluminated())
            if (Locator.GetFlashlight().IsFlashlightOn() || playerLightSensor.IsIlluminated())
            {
                var position = pair.Key.Camera.transform.position;
                if (__instance.AttachedObject.CheckPointInVisionCone(position))
                {
                    if (__instance.AttachedObject.CheckLineOccluded(__instance.AttachedObject._sightOrigin.position, position))
                    {
                        player.sensor.isPlayerOccluded = true;
                    }
                    else
                    {
                        player.sensor.isPlayerVisible = playerLightSensor.IsIlluminated() || Locator.GetFlashlight().IsFlashlightOn();
                        //player.sensor.isPlayerHeldLanternVisible = (lanternController.IsHeldByPlayer() && !lanternController.IsConcealed());
                        player.sensor.isPlayerHeldLanternVisible = Locator.GetFlashlight().IsFlashlightOn();
                    }
                }
            }

            /*if (!lanternController.IsHeldByPlayer() && __instance.AttachedObject.CheckPointInVisionCone(lanternController.GetLightPosition()) 
                && !__instance.AttachedObject.CheckLineOccluded(__instance.AttachedObject._sightOrigin.position, lanternController.GetLightPosition()))
            {
                player.sensor.isPlayerDroppedLanternVisible = true;
            }*/
        }

        if (!QSBCore.IsHost)
        {
            return;
        }

        var visiblePlayers = __instance._data.players.Values.Where(x => x.sensor.isPlayerVisible 
        || x.sensor.isPlayerHeldLanternVisible || x.sensor.inContactWithPlayer || x.sensor.isPlayerIlluminatedByUs);

        if (visiblePlayers.Count() == 0) // no players visible
        {
            visiblePlayers = __instance._data.players.Values.Where(x => x.sensor.isIlluminatedByPlayer);
        }

        if (visiblePlayers.Count() == 0) // no players lighting us
        {
            return;
        }

        var closest = visiblePlayers.MinBy(x => x.playerLocation.distance);

        if (__instance._data.interestedPlayer != closest)
        {
            __instance._data.interestedPlayer = closest;
            __instance.SendMessage(new ChangeInterestedPlayerMessage(closest.player.PlayerId));
        }
    }

    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(SingleLightSensor), nameof(SingleLightSensor.UpdateIllumination))]
    public static void FlaslightIlluminationFix(SingleLightSensor __instance)
    {
        __instance._illuminated = false;
        if (__instance._illuminatingDreamLanternList != null)
        {
            __instance._illuminatingDreamLanternList.Clear();
        }
        if (__instance._lightSources == null || __instance._lightSources.Count == 0)
        {
            return;
        }
        Vector3 vector = __instance.transform.TransformPoint(__instance._localSensorOffset);
        Vector3 vector2 = Vector3.zero;
        if (__instance._directionalSensor)
        {
            vector2 = __instance.transform.TransformDirection(__instance._localDirection).normalized;
        }
        for (int i = 0; i < __instance._lightSources.Count; i++)
        {
            if ((__instance._lightSourceMask & __instance._lightSources[i].GetLightSourceType()) == __instance._lightSources[i].GetLightSourceType() && __instance._lightSources[i].CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance))
            {
                LightSourceType lightSourceType = __instance._lightSources[i].GetLightSourceType();
                switch (lightSourceType)
                {
                    case LightSourceType.UNDEFINED:
                        {
                            OWLight2 owlight = __instance._lightSources[i] as OWLight2;
                            bool flag = owlight.GetLight().shadows != LightShadows.None && owlight.GetLight().shadowStrength > 0.5f;
                            if (owlight.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance) && !__instance.CheckOcclusion(owlight.transform.position, vector, vector2, flag))
                            {
                                __instance._illuminated = true;
                            }
                            break;
                        }
                    case LightSourceType.FLASHLIGHT:
                        {
                            Vector3 position = Locator.GetPlayerCamera().transform.position;
                            Vector3 vector3 = __instance.transform.position - position;
                            if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, vector3) <= __instance._maxSpotHalfAngle 
                                && !__instance.CheckOcclusion(position, vector, vector2, true))
                            {
                                __instance._illuminatingDreamLanternList.Add(ReferenceLocator.GetDreamLanternItem().GetLanternController());
                                __instance._illuminated = true;
                            }
                            break;
                        }
                    case LightSourceType.PROBE:
                        {
                            SurveyorProbe probe = Locator.GetProbe();
                            if (probe != null && probe.IsLaunched() && !probe.IsRetrieving() && probe.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance) && !__instance.CheckOcclusion(probe.GetLightSourcePosition(), vector, vector2, true))
                            {
                                __instance._illuminated = true;
                            }
                            break;
                        }
                    case LightSourceType.FLASHLIGHT | LightSourceType.PROBE:
                    case LightSourceType.FLASHLIGHT | LightSourceType.DREAM_LANTERN:
                    case LightSourceType.PROBE | LightSourceType.DREAM_LANTERN:
                    case LightSourceType.FLASHLIGHT | LightSourceType.PROBE | LightSourceType.DREAM_LANTERN:
                        break;
                    case LightSourceType.DREAM_LANTERN:
                        {
                            DreamLanternController dreamLanternController = __instance._lightSources[i] as DreamLanternController;
                            if (dreamLanternController.IsLit() && dreamLanternController.IsFocused(__instance._lanternFocusThreshold) 
                                && dreamLanternController.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance) 
                                && !__instance.CheckOcclusion(dreamLanternController.GetLightPosition(), vector, vector2, true))
                            {
                                __instance._illuminatingDreamLanternList.Add(dreamLanternController);
                                __instance._illuminated = true;
                            }
                            break;
                        }
                    case LightSourceType.SIMPLE_LANTERN:
                        foreach (OWLight2 owlight in __instance._lightSources[i].GetLights())
                        {
                            bool flag = owlight.GetLight().shadows != LightShadows.None && owlight.GetLight().shadowStrength > 0.5f;
                            float num = Mathf.Min(__instance._maxSimpleLanternDistance, __instance._maxDistance);
                            if (owlight.CheckIlluminationAtPoint(vector, __instance._sensorRadius, num) && !__instance.CheckOcclusion(owlight.transform.position, vector, vector2, flag))
                            {
                                __instance._illuminated = true;
                                break;
                            }
                        }
                        break;
                    default:
                        if (lightSourceType == LightSourceType.VOLUME_ONLY)
                        {
                            __instance._illuminated = true;
                        }
                        break;
                }
            }
        }
    }*/

    [HarmonyPrefix]
    [HarmonyPatch(typeof(QSBGhostGrabController), nameof(QSBGhostGrabController.GrabPlayer))]
    public static bool QSBGhostGrabFix(QSBGhostGrabController __instance, float speed, GhostPlayer player, bool remote = false)
    {
        if (!remote)
        {
            __instance.SendMessage(new GrabRemotePlayerMessage(speed, player.player.PlayerId));
        }

        var isLocalPlayer = player.player.IsLocalPlayer;

        if (isLocalPlayer)
        {
            __instance.AttachedObject.enabled = true;
            //__instance.AttachedObject._snappingNeck = !player.player.AssignedSimulationLantern.AttachedObject.GetLanternController().IsHeldByPlayer();
            __instance.AttachedObject._snappingNeck = true;
            __instance.AttachedObject._holdingInPlace = true;
            __instance.AttachedObject._grabMoveComplete = false;
            __instance.AttachedObject._extinguishStarted = false;
            __instance.AttachedObject._attachPoint.transform.parent = __instance.AttachedObject._origParent;
            __instance.AttachedObject._attachPoint.transform.position = Locator.GetPlayerTransform().position;
            __instance.AttachedObject._attachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
            __instance.AttachedObject._startLocalPos = __instance.AttachedObject._attachPoint.transform.localPosition;
            __instance.AttachedObject._startLocalRot = __instance.AttachedObject._attachPoint.transform.localRotation;
            __instance.AttachedObject._playerAttached = true;
            __instance.AttachedObject._attachPoint.AttachPlayer();
            GlobalMessenger.FireEvent("PlayerGrabbedByGhost");
            OWInput.ChangeInputMode(InputMode.None);
            ReticleController.Hide();
            Locator.GetDreamWorldController().SetActiveGhostGrabController(__instance.AttachedObject);
            __instance.AttachedObject._grabStartTime = Time.time;
            __instance.AttachedObject._grabMoveDuration = Mathf.Min(Vector3.Distance(__instance.AttachedObject._startLocalPos, __instance.AttachedObject._holdPoint.localPosition) / speed, 2f);
            RumbleManager.PlayGhostGrab();
            Achievement_Ghost.GotCaughtByGhost();
        }

        var effects = __instance.AttachedObject._effects.GetWorldObject<QSBGhostEffects>();

        if (QSBCore.IsHost)
        {
            if (__instance.AttachedObject._snappingNeck)
            {
                effects.PlaySnapNeckAnimation();
            }
            else
            {
                // todo : make this track grab counts, so we can use the fast animation
                effects.PlayBlowOutLanternAnimation();
            }
        }

        effects.AttachedObject.PlayGrabAudio(AudioType.Ghost_Grab_Contact);

        return false;
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
            if (ModMain.qsbEnabled)
            {
                QSBGhostBrain qsbBrain = brain.GetWorldObject<QSBGhostBrain>();
                QSBGhostData data = (QSBGhostData)typeof(QSBGhostBrain).GetField("_data", BindingFlags.NonPublic | 
                    BindingFlags.Public | BindingFlags.Instance).GetValue(qsbBrain);
                data.OnPlayerExitDreamWorld(QSBPlayerManager.LocalPlayer);
                qsbBrain.ChangeAction(null);
            }
            else
            {
                brain._data.OnPlayerExitDreamWorld();
                brain.ChangeAction(null);
            }
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
            ModMain.SetCondition("BT_HAS_SHRUBBERY", false);
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

        ReferenceLocator.GetFlashlightRuleset().OnExit();

        CageElevator elevator = ReferenceLocator.GetGhirdVillageBElevator();
        elevator._ghostInterface.SetStartingPosition(true);
        elevator.GoToDestination(elevator._destinations.Length - 1);
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