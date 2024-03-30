using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BandTogether;
public class SunpostDetector : MonoBehaviour
{
	[SerializeField] Transform target;
	[SerializeField] Transform hole;
	[SerializeField] EclipseDoorController doorController;

	Transform sunTransform;
	bool correctTime;
	bool opened;

	private void Start()
	{
        sunTransform = ModMain.Instance.nhAPI.GetPlanet("Jam 3 Sun").transform;
    }

	private void Update()
	{
		if (opened) return;

		if (sunTransform != null)
		{
			Vector3 planeNormal = Vector3.Cross(target.position - transform.position, transform.right);
			Vector3 sunVector = Vector3.ProjectOnPlane(target.position - sunTransform.position, planeNormal).normalized;
			Vector3 holeVector = Vector3.ProjectOnPlane(target.position - hole.transform.position, planeNormal).normalized;
			float dot = Vector3.Dot(sunVector, holeVector);
			if (!correctTime && dot > 0.999f)
			{
				correctTime = true;
				ModMain.SetCondition("SUNPOST_IN_RANGE", true);
            }
			else if (correctTime && dot <= 0.999f)
			{
				correctTime = false;
                ModMain.SetCondition("SUNPOST_IN_RANGE", false);
            }
        }

		if (PlayerData.GetPersistentCondition("OPEN_SUNPOST_DOOR"))
		{
			opened = true;
			doorController.CallOpenEvent();
		}
	}

	public void OpenDoor()
	{
		opened = true;
		doorController.CallOpenEvent();
	}
}
