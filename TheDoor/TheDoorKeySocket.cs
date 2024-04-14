using UnityEngine;

namespace BandTogether.TheDoor;

public class TheDoorKeySocket : OWItemSocket
{
    public event OWEvent.OWCallback OnKeyInserted;

    [SerializeField]
    private int numKeyFragments;

    private AudioSource _completionSfx = null;
    private KeySocketPromptDisplay _disabledPromptDisplay = null;
    private int _numInsertedFragments = 0;

    public override void Awake()
    {
        base.Awake();
        _acceptableType = KeyFragment.ItemType;
        _completionSfx = GetComponentInChildren<AudioSource>();
        _disabledPromptDisplay = GetComponentInChildren<KeySocketPromptDisplay>();
        
        OnSocketableDonePlacing += OnKeyFragmentDonePlacing;
        OnSocketablePlaced += OnKeyFragmentPlaced;
        
        EnableInteraction(false);
    }

    private void OnDestroy()
    {
        OnSocketableDonePlacing -= OnKeyFragmentDonePlacing;
        OnSocketablePlaced -= OnKeyFragmentPlaced;
    }

    public override void EnableInteraction(bool value)
    {
        base.EnableInteraction(value);
        _disabledPromptDisplay.SetInteractionEnabled(!value);
    }

    public void OnKeyFragmentPlaced(OWItem socketable)
    {
        // _numInsertedFragments won't have been incremented yet so check against one less num fragments
        if (numKeyFragments - 1 <= _numInsertedFragments) EnableInteraction(false);
    }

    public void OnKeyFragmentDonePlacing(OWItem socketable)
    {
        _numInsertedFragments += 1;
        if (_numInsertedFragments == numKeyFragments - 1) EnableInteraction(true);
        if (_numInsertedFragments < numKeyFragments) return;
        
        ModMain.WriteDebugMessage("key complete");
        Locator.GetShipLogManager().RevealFact("BT_KEY_COMPLETE");
        OnKeyInserted?.Invoke();
    }

    public void PlayCompletionSfx()
    {
        _completionSfx.Play();
    }
}