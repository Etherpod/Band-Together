using System;
using BandTogether.TheDoor;
using UnityEngine;

namespace BandTogether;

public class EndingController : MonoBehaviour
{
  [SerializeField] private TheDoorController doorController;
  [SerializeField] private Campfire fire;
  [SerializeField] private AudioSource[] nomaiInstruments;
  [SerializeField] private AudioSource[] ghirdInstruments;
  [SerializeField] private AudioSource pad;

  private void Awake()
  {
    fire.OnCampfireStateChange += OnFireStateChanged;
  }

  private void OnFireStateChanged(Campfire changedFire)
  {
    if (fire.GetState() == Campfire.State.LIT) OnFireLit();
  }

  private void OnEnterRoom()
  {
    
  }

  private void OnFireLit()
  {
    
  }
}