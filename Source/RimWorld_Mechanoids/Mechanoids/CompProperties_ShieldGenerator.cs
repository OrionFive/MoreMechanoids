using JetBrains.Annotations;
using Verse;

namespace MoreMechanoids;

public class CompProperties_ShieldGenerator : CompProperties
{
    [UsedImplicitly]
    public ThingDef shieldDef;

    public CompProperties_ShieldGenerator()
    {
        compClass = typeof(CompShieldGenerator);
    }
}