namespace BandTogether.TheDoor;

public class KeyFragment : OWItem
{
    public static readonly ItemType ItemType = (ItemType)256;

    public override string GetDisplayName() => "Key Fragment";

    private void Start()
    {
        onPickedUp += OnPickedUp;
    }

    public override bool IsAnimationPlaying()
    {
        return base.IsAnimationPlaying();
    }

    public override void PlaySocketAnimation()
    {
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