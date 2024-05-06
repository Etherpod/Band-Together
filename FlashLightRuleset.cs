using UnityEngine;

namespace BandTogether;
public class FlashlightRuleset : MonoBehaviour
{
    [SerializeField]
    private float fillLightRange = 90;
    [SerializeField]
    private float spotLightRange = 50;
    [SerializeField]
    private OWTriggerVolume _triggerVolume;

    private OWLight2 _fillLight;
    private OWLight2 _spotLight;
    private float _lastFillRange;
    private float _lastSpotRange;

    private void Start()
    {
        _triggerVolume.OnEntry += ctx => OnEntry();
        _triggerVolume.OnExit += ctx => OnExit();

        OWLight2[] lights = Locator.GetFlashlight().GetLights();
        for (int i = 0; i < lights.Length; i++)
        {
            ModMain.WriteDebugMessage(lights[i].name);
            if (lights[i].name == "Flashlight_FillLight")
            {
                _fillLight = lights[i];
            }
            else
            {
                _spotLight = lights[i];
            }
        }
    }

    public void OnEntry()
    {
        _lastFillRange = _fillLight.range;
        _lastSpotRange = _spotLight.range;
        _fillLight.range = fillLightRange;
        _spotLight.range = spotLightRange;
    }

    public void OnExit()
    {
        _fillLight.range = _lastFillRange;
        _spotLight.range = _lastSpotRange;
    }

    private void OnDestroy()
    {
        _triggerVolume.OnEntry -= ctx => OnEntry();
        _triggerVolume.OnExit -= ctx => OnExit();
    }
}
