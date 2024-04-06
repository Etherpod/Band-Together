namespace BandTogether;

public class TheDivineThrone : OWItemSocket
{
    public override void Awake()
    {
        base.Awake();
        _acceptableType = Shrubbery.ItemType;
    }

    public override void Start()
    {
        base.Start();

        OnSocketableDonePlacing += OnShrubberyReSeatedUponItsHolyArborealThrone;
        OnSocketableDoneRemoving += OnShrubberyRemovedFromItsHolyArborealThrone;
    }

    private void OnDestroy()
    {
        OnSocketableDonePlacing -= OnShrubberyReSeatedUponItsHolyArborealThrone;
        OnSocketableDoneRemoving += OnShrubberyRemovedFromItsHolyArborealThrone;
    }

    private void OnShrubberyReSeatedUponItsHolyArborealThrone(OWItem item)
    {
        if (!ModMain.GetPersistentCondition("START_STEAL_QUEST"))
        {
            base.EnableInteraction(false);
        }

        ModMain.SetPersistentCondition("FINISH_SHRUB_QUEST", true);
    }

    private void OnShrubberyRemovedFromItsHolyArborealThrone(OWItem item)
    {
        base.EnableInteraction(false);
        ReferenceLocator.GetShrubSocketNomai().EnableInteraction(true);
    }
}