namespace BandTogether.TheDoor;

public class KeyFragment : OWItem
{
    public static readonly ItemType ItemType = (ItemType)256;

    public override string GetDisplayName() => "Key Fragment";

    private void Start()
    {
        onPickedUp += OnPickedUp;
    }

    private void OnPickedUp(OWItem item)
    {
        FindObjectOfType<QuantumRockSolver>().OnPickedUp();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        onPickedUp -= OnPickedUp;
    }
}