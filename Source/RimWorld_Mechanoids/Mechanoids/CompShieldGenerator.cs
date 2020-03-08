using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class CompShieldGenerator : ThingComp
    {
        private CompProperties_ShieldGenerator Props => (CompProperties_ShieldGenerator) props;
        private Pawn Owner => (Pawn) parent;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad) return;

            if (!ApparelUtility.HasPartsToWear(Owner, Props.shieldDef)) return; // Might spawn with damaged parts
            
            ThingDef stuff = GenStuff.RandomStuffFor(Props.shieldDef);
            Thing thing = ThingMaker.MakeThing(Props.shieldDef, stuff);

            Apparel shield = (Apparel) GenSpawn.Spawn(thing, Owner.Position, Owner.Map);
            Owner.apparel.Wear(shield, false, true);
        }
    }
}
