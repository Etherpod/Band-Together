using BandTogether.Debug;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BandTogether.Util;

public class InputHandler : MonoBehaviour
{
	public void OnEnableDebugMenu(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		ModMain.Instance.ModHelper.Console.WriteLine("debug enable input received");
		ModMain.SetPersistentCondition("BAND_TOGETHER_DEBUG_ENABLED", true);
		ModMain.Instance.InitDebugMenu();
	}
}