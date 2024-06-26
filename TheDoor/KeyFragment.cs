﻿using System;
using UnityEngine;

namespace BandTogether.TheDoor;

[RequireComponent(typeof(Animator))]
public class KeyFragment : OWItem
{
	public static readonly ItemType ItemType = (ItemType)256;

	private static readonly int TriggerReveal = Animator.StringToHash("Reveal");
	private static readonly int TriggerMove = Animator.StringToHash("Move");
	private static readonly int TriggerInsert = Animator.StringToHash("Insert");
	private static readonly int TriggerActivate = Animator.StringToHash("Activate");
	private static readonly int StateInserted = Animator.StringToHash("Inserted");

	private Animator _animator = null;
	private bool _animating = false;

	public override string GetDisplayName() => "The Lost Shard";

	public override void Awake()
	{
		base.Awake();
		_type = ItemType;
		_animator = gameObject.GetRequiredComponent<Animator>();
	}

	public void Reveal()
	{
		_animator.SetTrigger(TriggerReveal);
	}

	public void ActivateDoor()
	{
		_animator.SetTrigger(TriggerActivate);
		_animating = false;
	}


	public override bool IsAnimationPlaying() =>
		_animating && _animator.GetCurrentAnimatorStateInfo(0).shortNameHash != StateInserted;

	public override void PlaySocketAnimation()
	{
		_animator.SetTrigger(TriggerInsert);
		_animating = true;
		_interactable = false;
	}

	public override void PickUpItem(Transform holdTranform)
	{
		base.PickUpItem(holdTranform);
		transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		transform.localPosition = new Vector3(0f, -0.4f, 0f);
		_animator.SetTrigger(TriggerMove);
		FindObjectOfType<KeySocketPromptDisplay>().UpdatePrompt(true);
	}

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        base.DropItem(position, normal, parent, sector, customDropTarget);
		KeySocketPromptDisplay prompt = FindObjectOfType<KeySocketPromptDisplay>();
        FindObjectOfType<KeySocketPromptDisplay>().UpdatePrompt(false);
    }
}