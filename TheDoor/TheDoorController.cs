using System;
using UnityEngine;

namespace BandTogether.TheDoor;

public class TheDoorController : MonoBehaviour
{
    private static readonly int Open = Animator.StringToHash("Open");

    [SerializeField] private TheDoorKeySocket theDoorKeySocket;
    [SerializeField] private KeyFragment keyFragment;
    [SerializeField] private AmbientMusicArea capitalAmbience;
    [SerializeField] private GameObject[] shards;

    private Animator _animator;
    private int _nextShard = 0;

    private void Awake()
    {
        _animator = gameObject.GetRequiredComponent<Animator>();

        theDoorKeySocket.OnKeyInserted += KeyInserted;
        ModMain.Instance.OnShardFound += condition =>
        {
            shards[_nextShard].SetActive(true);
            theDoorKeySocket.OnKeyFragmentPlaced(null);
            _nextShard++;
        };
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