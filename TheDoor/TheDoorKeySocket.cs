using UnityEngine;

namespace BandTogether.TheDoor;

public class TheDoorKeySocket : OWItemSocket
{
    public event OWEvent.OWCallback OnKeyInserted;

    [SerializeField]
    private int numKeyFragments;

    private AudioSource _completionSfx;
    private int _numInsertedFragments = 0;

    public override void Awake()
    {
        base.Awake();
        _acceptableType = KeyFragment.ItemType;
        _completionSfx = GetComponentInChildren<AudioSource>();
        
    }

    public override void Start()
    {
        base.Start();

        OnSocketableDonePlacing += OnKeyFragmentPlaced;
    }

    private void OnDestroy()
    {
        OnSocketableDonePlacing -= OnKeyFragmentPlaced;
    }

    private void OnKeyFragmentPlaced(OWItem socketable)
    {
        _numInsertedFragments += 1;
        ModMain.Instance.ModHelper.Console.WriteLine($"key fragments inserted: {_numInsertedFragments}");
        if (numKeyFragments <= _numInsertedFragments)
        {
            ModMain.Instance.ModHelper.Console.WriteLine("key complete");
            Locator.GetShipLogManager().RevealFact("KEY_COMPLETE");
            OnKeyInserted?.Invoke();
        }
    }

    public void PlayCompletionSfx()
    {
        _completionSfx.Play();
    }
}