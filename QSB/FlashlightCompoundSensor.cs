using QSB.Tools.FlashlightTool;
using System;
using UnityEngine;

namespace BandTogether.QSB;

public class FlashlightCompoundSensor : MonoBehaviour
{
    private CompoundLightSensor _lightSensor;

    private void Start()
    {
        _lightSensor = GetComponent<CompoundLightSensor>();
    }

    public bool IsIlluminatedByFlashlight(uint playerID)
    {
        if (_lightSensor._illuminatedCount == 0)
        {
            return false;
        }
        for (int i = 0; i < _lightSensor._childSensors.Length; i++)
        {
            if (_lightSensor._childSensors[i].GetComponent<FlashlightSensorData>().IsIlluminatedByFlashlight(playerID))
            {
                return true;
            }
        }
        return false;
    }
}
