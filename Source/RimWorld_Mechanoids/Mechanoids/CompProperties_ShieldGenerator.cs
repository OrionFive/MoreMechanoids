using Verse;

namespace MoreMechanoids
{
    public class CompProperties_ShieldGenerator : CompProperties
    {
        public ThingDef shieldDef;

        public CompProperties_ShieldGenerator()
        {
            compClass = typeof(CompShieldGenerator);
        }
    }
}
