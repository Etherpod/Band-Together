using System;
using BandTogether.TheDoor;
using UnityEngine;

namespace BandTogether;
public class QuantumRockSolver : MonoBehaviour
{
    private static readonly int Activated = Animator.StringToHash("Activated");
    private static readonly int PickedUp = Animator.StringToHash("PickedUp");
    
    [SerializeField] private QuantumSocket correctSocket;
    [SerializeField] private KeyFragment keyFragment;

    private SocketedQuantumObject _quantumController;
    private bool _puzzleSolved = false;
    private Animator _animator;

    private void Start()
    {
        _quantumController = GetComponent<SocketedQuantumObject>();
        _animator = GetComponentInChildren<Animator>();

        keyFragment.onPickedUp += OnPickedUp;
    }

    private void Update()
    {
        if (!_puzzleSolved && _quantumController._occupiedSocket == correctSocket && Locator.GetProbe().transform.parent == GetComponentInChildren<Collider>().transform)
        {
            _puzzleSolved = true;
            _quantumController.SetIsQuantum(false);
            //ModMain.Instance.ModHelper.Console.WriteLine("Solved puzzle");
            _animator.SetTrigger(Activated);
        }
    }

    public void OnPickedUp(OWItem item)
    {
        _animator.SetTrigger(PickedUp);
        DialogueConditionManager.SharedInstance.SetConditionState("FIFTH_SHARD", true);
        Locator.GetShipLogManager().RevealFact("FIFTH_SHARD_FOUND");
    }
}
