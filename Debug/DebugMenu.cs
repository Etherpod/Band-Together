﻿using BandTogether.Util;
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
	private Menu _conditionMenu;

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
		CreateConditionMenu();

		_menuAPI.PauseMenu_MakeSimpleButton("END SCREEN", _modMenu).onClick.AddListener(() =>
		{
			ModMain.Instance.ModHelper.Events.Unity.FireInNUpdates(() => ModMain.Instance.OnTriggerCampfireEnd(), 120);
			_modMenu.EnableMenu(false);
		});
	}

	private void CreateConditionMenu()
	{
		_conditionMenu = _menuAPI.PauseMenu_MakePauseListMenu("CONDITION TOOLS");
		_menuAPI.PauseMenu_MakeMenuOpenButton("CONDITIONS", _conditionMenu, _modMenu);

		AddConditionButton("SHARD // NOMAI A", "GOT_NOMAI_SHARD_A");
		AddConditionButton("SHARD // NOMAI B", "GOT_NOMAI_SHARD_B");
		AddConditionButton("SHARD // GHIRD A", "GOT_GHIRD_SHARD_A");
		AddConditionButton("SHARD // GHIRD B", "GOT_GHIRD_SHARD_B");
		AddConditionButton("DOOR // NOMAI A", "NOMAI_VILLAGE_A_TO_DOOR");
		AddConditionButton("DOOR // NOMAI B", "NOMAI_VILLAGE_B_TO_DOOR");
		AddConditionButton("DOOR // GHIRD A", "GHIRD_VILLAGE_A_TO_DOOR");
		AddConditionButton("DOOR // GHIRD B", "GHIRD_VILLAGE_B_TO_DOOR");
		AddConditionButton("CLANS LEAVE", "CLANS_LEAVE_DOOR");
	}

	private void AddConditionButton(string buttonText, string conditionName)
	{
		_menuAPI.PauseMenu_MakeSimpleButton(buttonText, _conditionMenu).onClick.AddListener(() =>
		{
			ModMain.SetCondition(conditionName, true);
		});
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