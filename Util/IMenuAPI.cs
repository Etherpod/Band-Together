﻿using UnityEngine;
using UnityEngine.UI;

namespace BandTogether.Util;

public interface IMenuAPI
{
	GameObject TitleScreen_MakeMenuOpenButton(string name, int index, Menu menuToOpen);

	GameObject TitleScreen_MakeSceneLoadButton(
		string name,
		int index,
		SubmitActionLoadScene.LoadableScenes sceneToLoad,
		PopupMenu confirmPopup = null
	);

	Button TitleScreen_MakeSimpleButton(string name, int index);
	GameObject PauseMenu_MakeMenuOpenButton(string name, Menu menuToOpen, Menu customMenu = null);

	GameObject PauseMenu_MakeSceneLoadButton(
		string name,
		SubmitActionLoadScene.LoadableScenes sceneToLoad,
		PopupMenu confirmPopup = null,
		Menu customMenu = null
	);

	Button PauseMenu_MakeSimpleButton(string name, Menu customMenu = null);
	Menu PauseMenu_MakePauseListMenu(string title);
	PopupMenu MakeTwoChoicePopup(string message, string confirmText, string cancelText);
	PopupInputMenu MakeInputFieldPopup(string message, string placeholderMessage, string confirmText, string cancelText);
	PopupMenu MakeInfoPopup(string message, string continueButtonText);
	void RegisterStartupPopup(string message);
}