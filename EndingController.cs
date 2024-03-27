using System;
using BandTogether.TheDoor;
using UnityEngine;

namespace BandTogether;

public class EndingController : MonoBehaviour
{
  [SerializeField] private EntrywayTrigger roomEntryway;
  [SerializeField] private Campfire fire;
  [SerializeField] private Animator musicAnimator;
  [SerializeField] private AudioSource[] nomaiInstruments;
  [SerializeField] private AudioSource[] ghirdInstruments;
  [SerializeField] private AudioSource pad;

  private bool _entered = false;
  private bool _fireLit = false;
  private static readonly int Play = Animator.StringToHash("Play");

  private void Awake()
  {
    fire.OnCampfireStateChange += OnFireStateChanged;
    roomEntryway.OnEntry += OnRoomEntered;
  }

  private void OnDestroy()
  {
    fire.OnCampfireStateChange -= OnFireStateChanged;
    roomEntryway.OnEntry -= OnRoomEntered;
  }

  private void OnRoomEntered(GameObject gameObject)
  {
    if (_entered || !gameObject.CompareTag("PlayerDetector")) return;

    _entered = true;
    DialogueConditionManager.SharedInstance.SetConditionState("SEARCHED_GREAT_DOOR", true);
  }

  private void OnFireStateChanged(Campfire changedFire)
  {
    if (_fireLit || fire.GetState() == Campfire.State.LIT) OnFireLit();
  }

  private void OnFireLit()
  {
    Invoke(nameof(NomaiToFire), 5);
    Invoke(nameof(GhirdToFire), 10);
    Invoke(nameof(StartPlaying), 10);
  }

  private void NomaiToFire()
  {
    DialogueConditionManager.SharedInstance.SetConditionState("NOMAI_TO_FIRE", true);
  }

  private void GhirdToFire()
  {
    DialogueConditionManager.SharedInstance.SetConditionState("GHIRD_TO_FIRE", true);
  }

  private void StartPlaying()
  {
    foreach (var nomaiInstrument in nomaiInstruments)
    {
      nomaiInstrument.Play();
    }
    
    musicAnimator.SetTrigger(Play);
  }
}