using UnityEngine;
using UnityEngine.Serialization;

namespace BandTogether.TheDoor;

public class KeySocketPromptDisplay : SingleInteractionVolume, IRaycastInteractable
{
	[SerializeField] private int interactRange;

	private bool _interactionEnabled = true;
	
	public override void Start()
	{
		base.Start();
		
		SetKeyCommandVisible(false);
		ChangePrompt("The Lost Shard cannot yet be inserted.");
	}

	public override void EnableInteraction()
	{
		base.EnableInteraction();
		_interactionEnabled = true;
	}

	public override void DisableInteraction()
	{
		base.DisableInteraction();
		_interactionEnabled = false;
	}

	public void Observe(RaycastHit hit)
	{
		_focused = _interactionEnabled && hit.distance < interactRange;
	}
}