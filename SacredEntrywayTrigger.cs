using System;
using UnityEngine;

namespace BandTogether;

[RequireComponent(typeof(EntrywayTrigger))]
public class SacredEntrywayTrigger : MonoBehaviour
{
    private EntrywayTrigger _entrywayTrigger;

    private GameObject _water;

    private void Awake()
    {
        _entrywayTrigger = GetComponent<EntrywayTrigger>();
        _entrywayTrigger.OnEntry += OnEnterSacredGround;
        _entrywayTrigger.OnExit += OnExitSacredGround;
    }

    public void LoadWaterObject(GameObject planet)
    {
        _water = planet.transform.Find("Sector/Water").gameObject;
    }

    private void OnEnterSacredGround(GameObject gameObject)
    {
        if (!gameObject.CompareTag("PlayerDetector")) return;

        _water.SetActive(false);
    }

    private void OnExitSacredGround(GameObject gameObject)
    {
        if (!gameObject.CompareTag("PlayerDetector")) return;

        _water.SetActive(true);
    }

    public void ForceSetEnabled(bool enabled)
    {
        _water.SetActive(enabled);
    }
}