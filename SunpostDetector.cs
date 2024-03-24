using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BandTogether;
public class SunpostDetector : MonoBehaviour
{
	[SerializeField] Transform target;
	[SerializeField] Transform hole;

	Transform sunTransform;

	private void Start()
	{
        sunTransform = ModMain.Instance.nhAPI.GetPlanet("Jam 3 Sun").transform;
    }

	private void Update()
	{
		if (sunTransform != null)
		{
			Vector3 planeNormal = Vector3.Cross(target.position - transform.position, transform.right);
			Vector3 sunVector = Vector3.ProjectOnPlane(target.position - sunTransform.position, planeNormal).normalized;
			Vector3 holeVector = Vector3.ProjectOnPlane(target.position - hole.transform.position, planeNormal).normalized;
			float dot = Vector3.Dot(sunVector, holeVector);
			if (dot > 0.999f)
			{
                //Change dialogue
            }
        }
	}
}
