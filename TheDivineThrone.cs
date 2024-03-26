using UnityEngine;

namespace BandTogether;

public class TheDivineThrone : OWItemSocket
{
  public override void Awake()
  {
    base.Awake();
    _acceptableType = Shrubbery.ItemType;
  }
  
  public override void Start()
  {
    base.Start();

    OnSocketableDonePlacing += OnShrubberyReSeatedUponItsHolyArborealThrone;
  }

  private void OnDestroy()
  {
    OnSocketableDonePlacing -= OnShrubberyReSeatedUponItsHolyArborealThrone;
  }

  private void OnShrubberyReSeatedUponItsHolyArborealThrone(OWItem item)
  {
    
  }
}