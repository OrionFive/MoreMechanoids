using RimWorld;
using Verse;

namespace MoreMechanoids;

public class CompShieldGenerator : ThingComp
{
    private CompProperties_ShieldGenerator Props => (CompProperties_ShieldGenerator)props;
    private Pawn Owner => (Pawn)parent;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        if (!ApparelUtility.HasPartsToWear(Owner, Props.shieldDef))
        {
            return; // Might spawn with damaged parts
        }

        if (Owner.apparel.WornApparel.Any(a => a.def == Props.shieldDef))
        {
            return; // Might already have an active shield
        }

        var stuff = GenStuff.RandomStuffFor(Props.shieldDef);
        var thing = ThingMaker.MakeThing(Props.shieldDef, stuff);

        var shield = (Apparel)GenSpawn.Spawn(thing, Owner.Position, Owner.Map);

        if (shield != null)
        {
            Owner.apparel.Wear(shield, false, true);
        }
    }
}