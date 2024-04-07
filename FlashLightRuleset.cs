using UnityEngine;

namespace BandTogether;
public class FlashlightRuleset : MonoBehaviour
{
    [SerializeField]
    float FillLight = 90;

    [SerializeField]
    float SpotLight = 50;

    [SerializeField]
    private OWTriggerVolume _triggerVolume;

    OWLight2 Flashlight_FillLight;
    OWLight2 Flashlight_SpotLight;

    private void Start()
    {
        _triggerVolume.OnEntry += ctx => OnEntry();
        _triggerVolume.OnExit += ctx => OnExit();

        Flashlight_FillLight = GameObject.Find("Player_Body")
            .GetComponentInChildren<PlayerCameraController>()
            .transform.Find("FlashlightRoot/Flashlight_BasePivot/Flashlight_WobblePivot/Flashlight_FillLight")
            .GetComponent<OWLight2>();
        Flashlight_SpotLight = GameObject.Find("Player_Body")
            .GetComponentInChildren<PlayerCameraController>()
            .transform.Find("FlashlightRoot/Flashlight_BasePivot/Flashlight_WobblePivot/Flashlight_SpotLight")
            .GetComponent<OWLight2>();
    }

    public void OnEntry()
    {
        Flashlight_FillLight.range = FillLight;
        Flashlight_SpotLight.range = SpotLight;
    }

    public void OnExit()
    {
        Flashlight_FillLight.range = 90;
        Flashlight_SpotLight.range = 50;
    }

    private void OnDestroy()
    {
        _triggerVolume.OnEntry -= ctx => OnEntry();
        _triggerVolume.OnExit -= ctx => OnExit();
    }
}
