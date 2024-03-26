using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BandTogether
{
    internal class FlashlightRuleset : MonoBehaviour
    {
        [SerializeField]
        float FillLight = 90;

        [SerializeField]
        float SpotLight = 50;

        [SerializeField]
        private Collider _trigger;

        private void OnTriggerEnter(Collider _trigger)
        {
            var Flashlight_FillLight = GameObject.Find("Player_Body/PlayerCamera/FlashlightRoot/Flashlight_BasePivot/Flashlight_WobblePivot/Flashlight_FillLight").GetComponent<OWLight2>();

            var Flashlight_SpotLight = GameObject.Find("Player_Body/PlayerCamera/FlashlightRoot/Flashlight_BasePivot/Flashlight_WobblePivot/Flashlight_SpotLight").GetComponent<OWLight2>();

            Flashlight_FillLight.range = FillLight;

            Flashlight_SpotLight.range = SpotLight;
        }
        private void OnTriggerExit(Collider _trigger)
        {
            var Flashlight_FillLight = GameObject.Find("Player_Body/PlayerCamera/FlashlightRoot/Flashlight_BasePivot/Flashlight_WobblePivot/Flashlight_FillLight").GetComponent<OWLight2>();

            var Flashlight_SpotLight = GameObject.Find("Player_Body/PlayerCamera/FlashlightRoot/Flashlight_BasePivot/Flashlight_WobblePivot/Flashlight_SpotLight").GetComponent<OWLight2>();

            Flashlight_FillLight.range = 90;

            Flashlight_SpotLight.range = 50;
        }
    }
}
