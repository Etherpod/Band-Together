using System;
using System.Collections;
using System.Collections.Generic;
using BandTogether.Util;
using UnityEngine;

namespace BandTogether.TheDoor;

public class TheDoorController : MonoBehaviour
{
    private static readonly IDictionary<ModMain.GroupType, int> ClanShards =
        new Dictionary<ModMain.GroupType, int>
        {
            { ModMain.GroupType.NomaiA, 0 },
            { ModMain.GroupType.NomaiB, 1 },
            { ModMain.GroupType.GhirdA, 2 },
            { ModMain.GroupType.GhirdB, 3 },
        };
    private static readonly int Open = Animator.StringToHash("Open");

    [SerializeField] private TheDoorKeySocket theDoorKeySocket;
    [SerializeField] private KeyFragment keyFragment;
    [SerializeField] private AmbientMusicArea capitalAmbience;
    [SerializeField] private Transform[] shards;

    private IDictionary<ModMain.GroupType, bool> _insertedShards =
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

    private void OnShardFound(ModMain.GroupType clan)
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
        _insertedShards[clan] = true;
        theDoorKeySocket.OnKeyFragmentPlaced(null);
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