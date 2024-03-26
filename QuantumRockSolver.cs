using System;
using UnityEngine;

namespace BandTogether;
public class QuantumRockSolver : MonoBehaviour
{
    [SerializeField] QuantumSocket correctSocket;

    SocketedQuantumObject quantumController;
    bool puzzleSolved = false;

    private void Start()
    {
        quantumController = GetComponent<SocketedQuantumObject>();
    }

    private void Update()
    {
        if (!puzzleSolved && quantumController._occupiedSocket == correctSocket && Locator.GetProbe().transform.parent == GetComponentInChildren<Collider>().transform)
        {
            puzzleSolved = true;
            quantumController.SetIsQuantum(false);
            ModMain.Instance.ModHelper.Console.WriteLine("Solved puzzle");
            GetComponentInChildren<Animator>().SetBool("Activated", true);
        }
    }

    public void OnPickedUp()
    {
        GetComponentInChildren<Animator>().SetBool("PickedUp", true);
        DialogueConditionManager.SharedInstance.SetConditionState("FIFTH_SHARD", true);
    }
}
