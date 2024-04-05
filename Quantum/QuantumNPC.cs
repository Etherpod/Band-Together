using System.Collections.Generic;
using BandTogether.Util;
using UnityEngine;
using static BandTogether.Quantum.QuantumGroup;

namespace BandTogether.Quantum;

public class QuantumNPC : SocketedQuantumObject
{
	[SerializeField] private QuantumGroup quantumGroup = Captial;
	[SerializeField] private QuantumNPCSocket[] targetSockets;

	private IDictionary<QuantumTarget, QuantumNPCSocket> _targets = null;

	private InteractReceiver _conversationInteract;
	private QuantumTarget _teleportTarget = QuantumTarget.Start;
	private bool _waitingToTeleport = false;
	private bool _ignoreVisibility = false;

	public QuantumTarget CurrentLocation => ((QuantumNPCSocket)_occupiedSocket)?.targetType ?? QuantumTarget.Start;

	public override void Awake()
	{
		base.Awake();

		ModMain.Instance.OnMoveGroup += OnMoveGroup;
		ModMain.Instance.OnMainQuest += OnMainQuest;

		_targets = targetSockets
			.SelectPair(socket => socket.targetType)
			.Flip()
			.ToDict();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		ModMain.Instance.OnMoveGroup -= OnMoveGroup;
		ModMain.Instance.OnMainQuest -= OnMainQuest;
	}

	public override void Start()
	{
		base.Start();

		_conversationInteract = GetComponentInChildren<InteractReceiver>();
		if (quantumGroup != Captial && _conversationInteract && !ModMain.GetPersistentCondition("MAIN_QUEST_START"))
		{
			_conversationInteract.SetInteractionEnabled(false);
		}
	}

	public void SetInteractionEnabled(bool interactionEnabled)
	{
		_conversationInteract.SetInteractionEnabled(interactionEnabled);
	}

	private void OnMainQuest()
	{
		if (quantumGroup != Captial && _conversationInteract)
			_conversationInteract.SetInteractionEnabled(true);
	}

	private void OnMoveGroup(QuantumGroup targetGroup, QuantumTarget targetType, bool ignoreVisibility)
	{
		if (targetGroup != quantumGroup) return;

		if (!_targets.ContainsKey(targetType))
		{
			ModMain.WriteDebugMessage($"{name} from group {quantumGroup} does not have a target of type {targetType}");
			return;
		}

		ModMain.WriteDebugMessage($"moving {name} to {targetType} [locked:{IsLocked()}]");

		if (_conversationInteract && targetType == QuantumTarget.Door) _conversationInteract.SetInteractionEnabled(false);

		_teleportTarget = targetType;
		_ignoreVisibility = ignoreVisibility;
		_waitingToTeleport = true;
		_wasLocked = true;
	}

	public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		if (!_waitingToTeleport) return true;

		// ModMain.WriteDebugMessage($"{name} trying to teleport to: {_teleportTarget}");
		_waitingToTeleport = false;

		var occupiedSocket = _occupiedSocket;

		MoveToSocket(_targets[_teleportTarget]);
		if (_ignoreVisibility || skipInstantVisibilityCheck) return true;

		var isVisible = CheckIllumination()
			? CheckVisibilityInstantly()
			: CheckPointInside(Locator.GetPlayerCamera().transform.position);

		// ModMain.WriteDebugMessage($"{name}'s {_teleportTarget} target visibility: {isVisible}");
		if (!isVisible) return true;

		// ModMain.WriteDebugMessage($"{name} retrying teleport");
		MoveToSocket(occupiedSocket);
		_waitingToTeleport = true;
		return false;
	}
}