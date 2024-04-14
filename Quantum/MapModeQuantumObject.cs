using System;
using UnityEngine;

namespace BandTogether;
public class MapModeQuantumObject : MonoBehaviour
{
    public bool ernestoRock;
    [SerializeField] QuantumSocket ernestoSocket = null;

    private bool disabledWater = false;

    public void OnTeleport()
    {
        ModMain.WriteDebugMessage(GetComponent<SocketedQuantumObject>().GetCurrentSocket());
        if (GetComponent<SocketedQuantumObject>().GetCurrentSocket() == ernestoSocket && !disabledWater)
        {
            ModMain.WriteDebugMessage("Disabled");
            disabledWater = true;
            ReferenceLocator.GetSacredEntryway().ForceSetEnabled(false);
        }
        else if (disabledWater)
        {
            ModMain.WriteDebugMessage("Enabled");
            disabledWater = false;
            ReferenceLocator.GetSacredEntryway().ForceSetEnabled(true);
        }
    }
}
