using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BandTogether.Util;

[RequireComponent(typeof(Animator))]
public class AnimatorLogger : MonoBehaviour
{
	[SerializeField] private string objectLogName = "";
	[SerializeField] private int layerToMonitor = 0;
	[SerializeField] private string[] states = {};
	[SerializeField] private string[] triggers = {};
	
	private readonly IDictionary<int, string> StateHashes = new Dictionary<int, string>();
	private readonly IDictionary<int, string> TriggerHashes = new Dictionary<int, string>();

	private Animator _animator = null;
	private int _lastStateHash = 0;

	private void Awake()
	{
		_animator = gameObject.GetRequiredComponent<Animator>();
	}

	private void Start()
	{
		states
			.SelectPair(Animator.StringToHash)
			.Flip()
			.AddAllTo(StateHashes);

		triggers
			.SelectPair(Animator.StringToHash)
			.Flip()
			.AddAllTo(TriggerHashes);
	}

	private void Update()
	{
		var stateNameHash = _animator.GetCurrentAnimatorStateInfo(layerToMonitor).shortNameHash;

		if (_lastStateHash == stateNameHash) return;

		_lastStateHash = stateNameHash;

		if (!StateHashes.TryGetValue(stateNameHash, out var stateName))
		{
			WriteLog($"unrecognized state name hash: {stateNameHash}");
			return;
		}
		
		WriteLog($"state changed to: {stateName}");
	}

	private void WriteLog(string message)
	{
		ModMain.WriteDebugMessage($"[ANIMLOG] {objectLogName} | {message}");
	}
}