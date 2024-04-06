using UnityEngine;

namespace BandTogether;
public class SunpostDetector : MonoBehaviour
{
	[SerializeField] Transform target;
	[SerializeField] Transform hole;
	[SerializeField] EclipseDoorController doorController;
	[SerializeField] EntrywayTrigger doorEntryway;
	[SerializeField] OWEmissiveRenderer gemEmissive;
	[SerializeField] float gemFadeTime;

	Transform sunTransform;
	bool correctTime = false;
	bool opened = false;
	bool waitToClose = false;
	bool playerInHouse = false;

	float gemFadeStart;
	float lastGemFade;
	bool fadeGemComplete = false;

	private void Start()
	{
        sunTransform = ModMain.Instance.nhAPI.GetPlanet("Jam 3 Sun").transform;
		gemEmissive.SetEmissiveScale(0f);
		doorEntryway.OnEntry += OnEntry;
		doorEntryway.OnExit += OnExit;
    }

	private void Update()
	{
        if (!fadeGemComplete)
		{
            if (correctTime)
            {
                float num = Mathf.InverseLerp(gemFadeStart, gemFadeStart + gemFadeTime, Time.time);
                gemEmissive.SetEmissiveScale(Mathf.Lerp(lastGemFade, 1f, num));
                if (num == 1)
                {
                    fadeGemComplete = true;
                }
            }
            else
            {
                float num = Mathf.InverseLerp(gemFadeStart, gemFadeStart + gemFadeTime * 2f, Time.time);
                gemEmissive.SetEmissiveScale(Mathf.Lerp(lastGemFade, 0f, num));
                if (num == 0)
                {
                    fadeGemComplete = true;
                }
            }
        }

        if (sunTransform != null)
        {
            Vector3 planeNormal = Vector3.Cross(target.position - transform.position, transform.right);
            Vector3 sunVector = Vector3.ProjectOnPlane(target.position - sunTransform.position, planeNormal).normalized;
            Vector3 holeVector = Vector3.ProjectOnPlane(target.position - hole.transform.position, planeNormal).normalized;
            float dot = Vector3.Dot(sunVector, holeVector);
            if (!correctTime && dot > 0.999f)
            {
                correctTime = true;
                fadeGemComplete = false;
                lastGemFade = gemEmissive.GetEmissiveScale();
                gemFadeStart = Time.time;
                ModMain.SetCondition("SUNPOST_IN_RANGE", true);
            }
            else if (correctTime && dot <= 0.999f)
            {
                correctTime = false;
                fadeGemComplete = false;
                lastGemFade = gemEmissive.GetEmissiveScale();
                gemFadeStart = Time.time;
                ModMain.SetCondition("SUNPOST_IN_RANGE", false);
            }
        }

        if (opened && !ModMain.GetPersistentCondition("OPEN_SUNPOST_DOOR"))
		{
			if (!playerInHouse)
			{
				CloseDoor();
			}
			else
			{
                waitToClose = true;
            }
		}
		else if (!opened && ModMain.GetPersistentCondition("OPEN_SUNPOST_DOOR"))
		{
			OpenDoor();
		}
	}

	public void OpenDoor()
	{
		opened = true;
		doorController.CallOpenEvent();
	}

	private void CloseDoor()
	{
        opened = false;
        doorController.CallCloseEvent();
    }

	private void OnEntry(GameObject obj)
	{
		if (obj.CompareTag("PlayerDetector"))
		{
			playerInHouse = true;
		}
	}

	private void OnExit(GameObject obj)
	{
		if (obj.CompareTag("PlayerDetector"))
		{
			playerInHouse = false;
			if (waitToClose)
			{
				opened = false;
				doorController.CallCloseEvent();
			}
		}
	}
}
