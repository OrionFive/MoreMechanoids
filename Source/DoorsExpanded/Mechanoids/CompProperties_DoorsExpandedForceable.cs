using MoreMechanoids;
using MoreMechanoidsDoorsExpanded.Mechanoids;

namespace MoreMechanoidsDoorsExpanded;

public class CompProperties_DoorsExpandedForceable : CompProperties_Forceable
{
    public CompProperties_DoorsExpandedForceable()
    {
        compClass = typeof(CompForceable);
        DoorHandler = new DoorsExpandedHandler();
    }
}