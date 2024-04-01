using System;
using System.Collections;
using System.Collections.Generic;
using BandTogether.Quantum;
using BandTogether.Util;
using UnityEngine;
using static BandTogether.Quantum.QuantumGroup;

namespace BandTogether.TheDoor;

public class TheDoorController : MonoBehaviour
{
    private static readonly IDictionary<QuantumGroup, int> ClanShards =
        new Dictionary<QuantumGroup, int>
        {
            { NomaiA, 0 },
            { NomaiB, 1 },
            { GhirdA, 2 },
            { GhirdB, 3 },
        };
    private static readonly int Open = Animator.StringToHash("Open");

    [SerializeField] private TheDoorKeySocket theDoorKeySocket;
    [SerializeField] private KeyFragment keyFragment;
    [SerializeField] private AmbientMusicArea capitalAmbience;
    [SerializeField] private Transform[] shards;

    private readonly IDictionary<QuantumGroup, bool> _insertedShards =
        ClanShards
            .Keys
            .SelectPair(clan => false)
            .ToDict();
    
    private Animator _animator;

    private void Awake()
    {
        if (shards.Length < 4) throw new Exception($"4 shards required but {shards.Length} found.");
        
        _animator = gameObject.GetRequiredComponent<Animator>();

        theDoorKeySocket.OnKeyInserted += KeyInserted;
        ModMain.Instance.OnShardFound += OnShardFound;
        
        shards.ForEach(shard => shard.localScale = Vector3.zero);
    }

    private void OnDestroy()
    {
        theDoorKeySocket.OnKeyInserted -= KeyInserted;
        ModMain.Instance.OnShardFound -= OnShardFound;
    }

    private void OnShardFound(QuantumGroup clan)
    {
        if (_insertedShards[clan])
        {
            ModMain.WriteDebugMessage($"shard was already inserted for: {clan}");
            return;
        }
        
        ModMain.WriteDebugMessage($"shard inserted for: {clan}");
        
        var clanShard = ClanShards[clan];
        ModMain.WriteDebugMessage($"clanShard: {clanShard}");
        
        shards[clanShard].localScale = Vector3.one;
        theDoorKeySocket.OnKeyFragmentDonePlacing(null);
        
        _insertedShards[clan] = true;
    }

    private void KeyInserted()
    {
        _animator.SetTrigger(Open);
        keyFragment.ActivateDoor();
        capitalAmbience.FadeOut();
    }

    public void PlayKeyCompletionSfx()
    {
        theDoorKeySocket.PlayCompletionSfx();
    }
}