using System;
using BandTogether.Util;
using UnityEngine;

namespace BandTogether.TheDoor;

public class TheDoorController : MonoBehaviour
{
    private static readonly int Open = Animator.StringToHash("Open");

    [SerializeField] private TheDoorKeySocket theDoorKeySocket;
    [SerializeField] private KeyFragment keyFragment;
    [SerializeField] private AmbientMusicArea capitalAmbience;
    [SerializeField] private Transform[] shards;

    private Animator _animator;
    private int _nextShard = 0;

    private void Awake()
    {
        _animator = gameObject.GetRequiredComponent<Animator>();

        theDoorKeySocket.OnKeyInserted += KeyInserted;
        ModMain.Instance.OnShardFound += () =>
        {
            // ModMain.WriteMessage($"shards: {shards}");
            if (shards.Length <= _nextShard) return;
            
            ModMain.WriteDebugMessage("shard inserted");
            shards[_nextShard].localScale = Vector3.one;
            theDoorKeySocket.OnKeyFragmentPlaced(null);
            _nextShard++;
        };
        
        // ModMain.WriteMessage($"shards: {shards}");
        // shards.ForEach(shard => shard.localScale = Vector3.zero);
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