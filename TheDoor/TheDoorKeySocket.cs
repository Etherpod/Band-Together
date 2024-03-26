using UnityEngine;

namespace BandTogether.TheDoor;

public class TheDoorKeySocket : OWItemSocket
{
    public event OWEvent.OWCallback OnKeyInserted;

    [SerializeField]
    private int numKeyFragments;

    private int _numInsertedFragments = 0;

    public override void Awake()
    {
        base.Awake();
        _acceptableType = KeyFragment.ItemType;
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

    public override OWItem RemoveFromSocket()
    {
        ((ScrollItem)_socketedItem).HideNomaiText();
        return base.RemoveFromSocket();
    }

    private void OnKeyFragmentPlaced(OWItem socketable)
    {
        _numInsertedFragments += 1;
        if (numKeyFragments <= _numInsertedFragments)
        {
            OnKeyInserted?.Invoke();
        }
    }
}