using BandTogether.Util;
using UnityEngine;

namespace BandTogether.Debug;

public class DebugMenu
{
	private static DebugMenu _instance;

	public static DebugMenu Instance => _instance;

	private readonly GameObject _planet;

	private IMenuAPI _menuAPI;
	private Menu _modMenu;
	private Menu _telepoMenu;

	private DebugMenu(GameObject planet)
	{
		_planet = planet;
	}

	public static DebugMenu InitMenu(GameObject planet)
	{
		if (_instance is not null) return _instance;
		
		_instance = new DebugMenu(planet);
		_instance.LoadMenu();

		return _instance;
	}

	private void LoadMenu()
	{
		_menuAPI = ModMain.Instance.ModHelper.Interaction.TryGetModApi<IMenuAPI>("_nebula.MenuFramework");

		_modMenu = _menuAPI.PauseMenu_MakePauseListMenu("BAND TOGETHER DEBUG ACTIONS");
		_menuAPI.PauseMenu_MakeMenuOpenButton("BAND-TOGETHER DEBUG ACTIONS", _modMenu);

		CreateTelepoMenu();
	}

	private void CreateTelepoMenu()
	{
		_telepoMenu = _menuAPI.PauseMenu_MakePauseListMenu("TELEPORT DESTINATIONS");
		_menuAPI.PauseMenu_MakeMenuOpenButton("TELEPORT", _telepoMenu, _modMenu);

		AddTeleportButton("NORTH POLE", "NorthPole");
		AddTeleportButton("SOUTH POLE", "SouthPole");
		AddTeleportButton("THE DOOR", "TheDoor");
		AddTeleportButton("NOMAI // COCKPIT", "NomaiCockpit");
		AddTeleportButton("NOMAI // OTHER", "NomaiOther");
		AddTeleportButton(
			"BIRB // FOLLOWERS OF ITS GRAND EPHEMERAL ARBOREAL ILLUMINATING ETERNAL SOVEREIGN CELESTIAL TRANQUIL BEARER, THE SACRED SHRUBBERY",
			"GhirdShrubbery");
		AddTeleportButton("BIRB // LOGIC", "GhirdLogic");
	}

	private void AddTeleportButton(string buttonText, string targetName)
	{
		_menuAPI.PauseMenu_MakeSimpleButton(buttonText, _telepoMenu).onClick.AddListener(() =>
		{
			TeleportPlayer(targetName);
			_telepoMenu.EnableMenu(false);
			_modMenu.EnableMenu(false);
		});
	}

	private void TeleportPlayer(string target)
	{
		var playerBody = Locator.GetPlayerBody();
		// var playerCamera = Locator.GetPlayerCamera();
		var destination = _planet.transform.Find($"Sector/JamPlanet/Debug/TeleportDestinations/{target}");
		var planetBody = _planet.GetComponent<OWRigidbody>();

		var targetRotation = destination.rotation;
		var targetPosition = destination.position + 2 * (targetRotation * Vector3.up);
		var targetVelocity = planetBody.GetVelocity();

		playerBody.SetPosition(targetPosition);
		playerBody.SetRotation(targetRotation);
		playerBody.SetVelocity(targetVelocity);

		// playerCamera.transform.rotation = targetRotation;
	}
}