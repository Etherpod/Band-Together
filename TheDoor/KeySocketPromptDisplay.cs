using UnityEngine;

namespace BandTogether.TheDoor;

public class KeySocketPromptDisplay : SingleInteractionVolume, IRaycastInteractable
{
	[SerializeField] private int interactRange;

	private bool _interactionEnabled = true;
	
	public override void Start()
	{
		base.Start();
		
		SetKeyCommandVisible(false);
		ChangePrompt("");
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

	public void UpdatePrompt(bool enabled)
	{
		if (!enabled)
		{
            ChangePrompt("The Fifth Shard cannot be inserted yet.");
        }
		else
		{
			ChangePrompt("");
		}
	}

	public void Observe(RaycastHit hit)
	{
		_focused = _interactionEnabled && hit.distance < interactRange;
	}
}