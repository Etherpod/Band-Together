using NAudio.Wave;
using System;
using UnityEngine;

namespace BandTogether;
public class QuantumNPC : SocketedQuantumObject
{
	[Tooltip("It will move through the targets in order every time the event is called.")]
	[SerializeField] Transform[] targetList;

	int targetIndex = 0;
	bool waitingToTeleport = false;

	private new void Start()
	{
		ModMain.Instance.OnMoveVillage += OnMoveVillage;
	}

	private void OnMoveVillage(string target)
	{
		if (target == "NOMAI_B")
		{
			waitingToTeleport = true;
		}
	}

	private new void Update()
	{
		if (waitingToTeleport && !IsLocked())
		{
			waitingToTeleport = false;
			transform.position = targetList[targetIndex].position;
			transform.rotation = targetList[targetIndex].rotation;
			targetIndex++;

			if (targetIndex >= targetList.Length)
			{
				targetIndex = 0;
			}
		}
	}

    private void OnDisable()
	{
        ModMain.Instance.OnMoveVillage -= OnMoveVillage;
    }
}
