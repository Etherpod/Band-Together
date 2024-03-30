namespace BandTogether;

public class ShrubberySocketNomai : OWItemSocket
{
    public override void Awake()
    {
        base.Awake();
        _acceptableType = Shrubbery.ItemType;
    }

    public override void Start()
    {
        base.Start();

        OnSocketableDonePlacing += OnInsert;

        base.EnableInteraction(false);
    }

    private void OnInsert(OWItem item)
    {
        ModMain.SetPersistentCondition("SHRUB_GIVEN_TO_NOMAI", true);
        base.EnableInteraction(false);
    }
}
