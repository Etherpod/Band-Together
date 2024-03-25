using System.Collections.Generic;
using System;
using UnityEngine;

namespace BandTogether;
public class QuantumNPC : SocketedQuantumObject
{
	[SerializeField] Transform[] targetList;
    [SerializeField] GroupType groupType;

	int targetIndex = 0;
	bool waitingToTeleport = false;

    public enum GroupType
    {
        NOMAI_A,
        NOMAI_B,
        GHIRD_A,
        GHIRD_B,
        CAPTIAL
    }

	public override void Start()
	{
		base.Start();
		ModMain.Instance.OnMoveGroup += OnMoveGroup;
	}

	private void OnMoveGroup(string target)
	{
		if (target == groupType.ToString())
		{
			waitingToTeleport = true;
		}
	}

	public override void Update()
	{
		base.Update();
		if (waitingToTeleport && !IsLocked())
		{
			waitingToTeleport = false;
			transform.position = targetList[targetIndex].position;
			transform.rotation = targetList[targetIndex].rotation;
			targetIndex++;

			if (targetIndex >= targetList.Length)
			{
				targetIndex = 0;
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

    private void OnDisable()
	{
        ModMain.Instance.OnMoveGroup -= OnMoveGroup;
    }
}
