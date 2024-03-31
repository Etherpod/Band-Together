using System.Collections.Generic;
using System;
using UnityEngine;

namespace BandTogether;

public class QuantumNPC : SocketedQuantumObject
{
    [SerializeField] private QuantumSocket[] targetList;
    [SerializeField] private ModMain.GroupType groupType;

    private bool _actQuantum = true;
    private InteractReceiver _conversationInteract;
    private int _targetIndex = 0;
    private bool _waitingToTeleport = false;

    public override void Start()
    {
        if (this._newlyObscuredSocketProbability > 0f)
        {
            for (int i = 0; i < this._socketList.Count; i++)
            {
                ModMain.WriteDebugMessage("Testing socket: " + _socketList[i].name);
                if (this._socketList[i].GetVisibilityObject() != null)
                {
                    ModMain.WriteDebugMessage("Visiblity object: " + _socketList[i].name);
                    this._socketList[i].OnNewlyObscured += this.OnSocketObscured;
                }
            }
        }
        if (this._alignWithGravity)
        {
            this._gravityVolume = base.GetComponentInParent<OWRigidbody>().GetAttachedGravityVolume();
        }
        base.Start();

        // ModMain.Instance.ModHelper.Console.WriteLine($"{groupType} awakened");
        ModMain.Instance.OnMoveGroup += OnMoveGroup;
        ModMain.Instance.OnMainQuest += OnMainQuest;
        //targetList[_targetIndex].OnNewlyObscured += OnSocketObscured;

        _conversationInteract = GetComponentInChildren<InteractReceiver>();
        if (groupType != ModMain.GroupType.Captial && _conversationInteract && !PlayerData.GetPersistentCondition("MAIN_QUEST_START"))
        {
            _conversationInteract.SetInteractionEnabled(false);
        }

        //ModMain.WriteDebugMessage(targetList[_targetIndex].GetVisibilityObject());
    }

    private void OnMainQuest()
    {
        if (groupType != ModMain.GroupType.Captial && _conversationInteract)
        {
            _conversationInteract.SetInteractionEnabled(true);
        }
    }

    private void OnMoveGroup(ModMain.GroupType targetGroup, bool shouldActQuantum)
    {
        if (targetGroup != groupType) return;

        if (_conversationInteract) _conversationInteract.SetInteractionEnabled(false);

        _actQuantum = shouldActQuantum;
        _waitingToTeleport = true;
    }

    public override void Update()
    {
        base.Update();
        if (targetList[_targetIndex].GetVisibilityObject() != null && targetList[_targetIndex].GetVisibilityObject().IsVisible())
        {
            ModMain.WriteDebugMessage("Socket in view: " + targetList[_targetIndex].name);
        }
    }

    //Just getting rid of the error logs when there are no sockets (nothing actually breaks)
    public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
    {
        if (!_waitingToTeleport) { return false; }

        _waitingToTeleport = false;

        for (int i = 0; i < this._childSockets.Count; i++)
        {
            if (this._childSockets[i].IsOccupied())
            {
                return false;
            }
        }
        if (this._socketList.Count < 1)
        {
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
            this.MoveToSocket(list[_targetIndex]);
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
            list.RemoveAt(_targetIndex);
            _targetIndex++;
            if (_targetIndex >= targetList.Length)
            {
                _targetIndex = 0;
            }
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
        //targetList[_targetIndex].OnNewlyObscured -= OnSocketObscured;
    }
}

