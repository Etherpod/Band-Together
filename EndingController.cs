using System;
using BandTogether.TheDoor;
using BandTogether.Util;
using UnityEngine;

namespace BandTogether;

public class EndingController : MonoBehaviour
{
  [SerializeField] private EntrywayTrigger roomEntryway = null;
  [SerializeField] private Campfire fire = null;
  [SerializeField] private Animator musicAnimator = null;
  [SerializeField] private AudioSource[] nomaiInstruments = null;
  [SerializeField] private AudioSource[] ghirdInstruments = null;
  [SerializeField] private AudioSource pad = null;

  private static readonly int Play = Animator.StringToHash("Play");

  private int _moves = 0;
  private bool _fireLit = false;

  private void Awake()
  {
    fire.OnCampfireStateChange += OnFireStateChanged;
    roomEntryway.OnEntry += OnRoomEntered;
    ModMain.Instance.OnMoveGroup += OnClansMove;
  }

  private void OnDestroy()
  {
    fire.OnCampfireStateChange -= OnFireStateChanged;
    roomEntryway.OnEntry -= OnRoomEntered;
  }

  private void OnRoomEntered(GameObject enteringObject)
  {
    if (!enteringObject.CompareTag("PlayerDetector")) return;

    fire.SetInteractionEnabled(false);
    roomEntryway.OnEntry -= OnRoomEntered;
    DialogueConditionManager.SharedInstance.SetConditionState("SEARCHED_GREAT_DOOR", true);
  }

  private void OnClansMove(QuantumNPC.GroupType target, bool shouldActQuatum)
  {
    if (target != QuantumNPC.GroupType.NomaiA) return;
    
    _moves++;
    if (_moves != 2) return;

    fire.SetInteractionEnabled(true);
  }

  private void OnFireStateChanged(Campfire changedFire)
  {
    if (!_fireLit && fire.GetState() == Campfire.State.LIT) OnFireLit();
  }

  private void OnFireLit()
  {
    _fireLit = true;
    Invoke(nameof(DoorkeeperToFire), 5);
    Invoke(nameof(NomaiToFire), 10);
    Invoke(nameof(GhirdToFire), 15);
    Invoke(nameof(StartPlaying), 10);
  }

  private void DoorkeeperToFire()
  {
    DialogueConditionManager.SharedInstance.SetConditionState("DOORKEEPER_TO_FIRE", true);
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
    nomaiInstruments.ForEach(instrument =>
    {
      instrument.Play();
      instrument.volume = 0;
    });
    ghirdInstruments.ForEach(instrument =>
    {
      instrument.Play();
      instrument.volume = 0;
    });
    
    musicAnimator.SetTrigger(Play);
    ModMain.Instance.Invoke(nameof(ModMain.OnTriggerCampfireEnd), 40);
  }

  // public void AndSoTheStoryComesToAnEnd()
  // {
  //   ModMain.Instance.OnTriggerCampfireEnd();
  // }
}