﻿using System;
using BandTogether.Quantum;
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
    ModMain.SetCondition("SEARCHED_GREAT_DOOR", true);
  }

  private void OnClansMove(QuantumGroup targetGroup, QuantumTarget targetType, bool ignoreVisibility)
  {
    if (targetType != QuantumTarget.Away) return;

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
    ModMain.SetCondition("DOORKEEPER_TO_FIRE", true);
  }

  private void NomaiToFire()
  {
    ModMain.SetCondition("NOMAI_TO_FIRE", true);
  }

  private void GhirdToFire()
  {
    ModMain.SetCondition("GHIRD_TO_FIRE", true);
  }

  private void StartPlaying()
  {
    nomaiInstruments.ForEach(instrument =>
    {
      instrument.volume = 0;
      instrument.Play();
    });
    ghirdInstruments.ForEach(instrument =>
    {
      instrument.volume = 0;
      instrument.Play();
    });
    
    musicAnimator.SetTrigger(Play);
    // ModMain.Instance.Invoke(nameof(ModMain.OnTriggerCampfireEnd), 40);
  }
}