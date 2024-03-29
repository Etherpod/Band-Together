using System.Collections.Generic;
using System;
using UnityEngine;

namespace BandTogether;
public class QuantumNPC : SocketedQuantumObject
{
	[SerializeField] private Transform[] targetList;
	[SerializeField] private GroupType groupType;

    private bool _actQuantum = true;
    private GameObject _conversationToEnable;

	public enum GroupType
	{
		NomaiA,
		NomaiB,
		GhirdA,
		GhirdB,
		Captial
	}

	private int _targetIndex = 0;
	private bool _waitingToTeleport = false;

	public override void Awake()
	{
		base.Awake();
		// ModMain.Instance.ModHelper.Console.WriteLine($"{groupType} awakened");
		ModMain.Instance.OnMoveGroup += OnMoveGroup;
        ModMain.Instance.OnMainQuest += OnMainQuest;
	}

    public override void Start()
    {
        base.Start();
        if (groupType != GroupType.Captial && GetComponentInChildren<InteractReceiver>() && !PlayerData.GetPersistentCondition("MAIN_QUEST_START"))
        {
            _conversationToEnable = GetComponentInChildren<InteractReceiver>().gameObject;
            _conversationToEnable.SetActive(false);
        }
    }

    private void OnMainQuest()
    {
        if (groupType != GroupType.Captial)
        {
            _conversationToEnable.SetActive(true);
        }
    }

	private void OnMoveGroup(GroupType targetGroup, bool shouldActQuantum)
	{
		// ModMain.Instance.ModHelper.Console.WriteLine($"{groupType} asked to move for: {targetGroup}");
		if (targetGroup != groupType) return;
		// ModMain.Instance.ModHelper.Console.WriteLine($"{groupType} moving");

		_actQuantum = shouldActQuantum;
		_waitingToTeleport = true;
	}

	public override void Update()
	{
		base.Update();
		if (_waitingToTeleport && (!IsLocked() || !_actQuantum))
		{
			_waitingToTeleport = false;
			transform.position = targetList[_targetIndex].position;
			transform.rotation = targetList[_targetIndex].rotation;
			_targetIndex++;

			if (_targetIndex >= targetList.Length)
			{
				_targetIndex = 0;
			}
		}
	}

    //Just getting rid of the error logs when there are no sockets (nothing actually breaks)
	public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
        for (int i = 0; i < this._childSockets.Count; i++)
        {
            if (this._childSockets[i].IsOccupied())
            {
                return false;
            }
        }
        if (this._socketList.Count <= 1)
        {
            //Debug.LogError("Not enough quantum sockets in list!", this);
            //Debug.Break();
            return false;
        }
        List<QuantumSocket> list = new List<QuantumSocket>();
        for (int j = 0; j < this._socketList.Count; j++)
        {
            if (!this._socketList[j].IsOccupied() && this._socketList[j].IsActive())
            {
                list.Add(this._socketList[j]);
            }
        }
        if (list.Count == 0)
        {
            return false;
        }
        if (this._recentlyObscuredSocket != null)
        {
            this.MoveToSocket(this._recentlyObscuredSocket);
            this._recentlyObscuredSocket = null;
            return true;
        }
        QuantumSocket occupiedSocket = this._occupiedSocket;
        for (int k = 0; k < 20; k++)
        {
            int num = UnityEngine.Random.Range(0, list.Count);
            this.MoveToSocket(list[num]);
            if (skipInstantVisibilityCheck)
            {
                return true;
            }
            bool flag;
            if (this.IsPlayerEntangled())
            {
                flag = this.CheckIllumination();
            }
            else
            {
                flag = (this.CheckIllumination() ? base.CheckVisibilityInstantly() : base.CheckPointInside(Locator.GetPlayerCamera().transform.position));
            }
            if (!flag)
            {
                return true;
            }
            list.RemoveAt(num);
            if (list.Count == 0)
            {
                break;
            }
        }
        this.MoveToSocket(occupiedSocket);
        return false;
    }

    public override void OnDestroy()
	{
        base.OnDestroy();
        ModMain.Instance.OnMoveGroup -= OnMoveGroup;
        ModMain.Instance.OnMainQuest -= OnMainQuest;
    }
}
