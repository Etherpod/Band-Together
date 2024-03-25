using System;
using BandTogether.TheDoor;
using UnityEngine;

namespace BandTogether;

public class EndingController : MonoBehaviour
{
  [SerializeField]
  private TheDoorController doorController;
  
  [SerializeField]
  private Campfire fire;

  private void Awake()
  {
    fire.OnCampfireStateChange += OnFireStateChanged;
    doorController.OnOpening += OnDoorOpening;
  }

  private void OnFireStateChanged(Campfire changedFire)
  {
    if (fire.GetState() == Campfire.State.LIT) OnFireLit();
  }

  private void OnDoorOpening()
  {
    
  }

  private void OnFireLit()
  {
    
  }
}