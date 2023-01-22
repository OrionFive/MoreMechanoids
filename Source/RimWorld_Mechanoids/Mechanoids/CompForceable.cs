using Verse;

namespace MoreMechanoids;

/// <summary>
///     So doors can be forced open.
/// </summary>
public class CompForceable : ThingComp
{
    public bool forcedOpen;
    private bool originalHoldOpenValue;
    private CompProperties_Forceable Props => (CompProperties_Forceable)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref forcedOpen, "forcedOpen");
        Scribe_Values.Look(ref originalHoldOpenValue, "originalHoldOpenValue");
    }

    public void Force()
    {
        if (forcedOpen)
        {
            return;
        }

        Props.doorHandler.Force(parent, ref originalHoldOpenValue, ref forcedOpen);
    }

    public void Fix()
    {
        if (!forcedOpen)
        {
            return;
        }

        Props.doorHandler.Fix(parent, originalHoldOpenValue, ref forcedOpen);
    }
}