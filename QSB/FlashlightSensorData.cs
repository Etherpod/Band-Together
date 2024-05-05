using QSB;
using QSB.API;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BandTogether.QSB;

public class FlashlightSensorData : MonoBehaviour
{
    private List<uint> _illuminatingFlashlightList = new();
    private SingleLightSensor _lightSensor;

    private void Start()
    {
        _lightSensor = gameObject.GetComponent<SingleLightSensor>();
    }
    
    public void AddFlashlightID(uint playerID)
    {
        _illuminatingFlashlightList.Add(playerID);
    }

    public void ClearFlashlightList()
    {
        _illuminatingFlashlightList.Clear();
    }

    public void SetFlashlightList(uint[] playerIDList)
    {
        if (QSBCore.IsHost)
        {
            //ModMain.WriteDebugMessage("Info: " + playerIDList.Length);
        }
        _illuminatingFlashlightList.Clear();
        _illuminatingFlashlightList.AddRange(playerIDList);
        //ModMain.WriteDebugMessage("After length: " + _illuminatingFlashlightList.Count);
    }
    
    public void SendList(uint[] playerIDList)
    {
        if (!_illuminatingFlashlightList.SequenceEqual(playerIDList))
        {
            SetFlashlightList(playerIDList);
            ModMain.qsbAPI.SendMessage("flashlightIDs", (playerIDList, _lightSensor.GetWorldObject<QSBLightSensor>().ObjectId), receiveLocally: false);
        }
    }

    public bool IsIlluminatedByFlashlight(uint playerID)
    {
        if (!_lightSensor._illuminated)
        {
            return false;
        }
        for (int i = 0; i < _illuminatingFlashlightList.Count; i++)
        {
            //ModMain.WriteDebugMessage(i + ": " + _illuminatingFlashlightList[i]);
            //ModMain.WriteDebugMessage("Player ID: " + playerID);
            if (_illuminatingFlashlightList[i] == playerID)
            {
                return true;
            }
        }
        return false;
    }
}
