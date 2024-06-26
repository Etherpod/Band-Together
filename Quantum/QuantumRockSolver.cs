﻿using BandTogether.TheDoor;
using UnityEngine;

namespace BandTogether.Quantum;
public class QuantumRockSolver : MonoBehaviour
{
    private static readonly int Activated = Animator.StringToHash("Activated");
    private static readonly int PickedUp = Animator.StringToHash("PickedUp");
    
    [SerializeField] private QuantumSocket correctSocket;
    [SerializeField] private KeyFragment keyFragment;
    [SerializeField] private Transform animationRoot;

    private SocketedQuantumObject _quantumController;
    private bool _puzzleSolved = false;
    private Animator _animator;

    private void Start()
    {
        _quantumController = GetComponent<SocketedQuantumObject>();
        _animator = GetComponent<Animator>();

        keyFragment.onPickedUp += OnPickedUp;
    }

    private void Update()
    {
        if (!_puzzleSolved && _quantumController._occupiedSocket == correctSocket && Locator.GetProbe().transform.parent == GetComponentInChildren<Collider>().transform)
        {
            _puzzleSolved = true;
            _quantumController.SetIsQuantum(false);
            _animator.SetTrigger(Activated);
            keyFragment.transform.parent = animationRoot;
            keyFragment.transform.localPosition = Vector3.zero;
            keyFragment.transform.localRotation = Quaternion.identity;
            keyFragment.transform.localScale = Vector3.one;
        }
    }

    public void Reveal()
    {
        keyFragment.Reveal();
    }

    public void OnPickedUp(OWItem item)
    {
        _animator.SetTrigger(PickedUp);
        ModMain.SetCondition("BT_FIFTH_SHARD", true);
        if (ModMain.AllFifthShardRumors())
        {
            Locator.GetShipLogManager().RevealFact("BT_FIFTH_SHARD_FOUND");
        }
        else
        {
            Locator.GetShipLogManager().RevealFact("BT_FIFTH_SHARD_ACCIDENT");
        }
    }

    private void OnDestroy()
    {
        keyFragment.onPickedUp -= OnPickedUp;
    }
}
