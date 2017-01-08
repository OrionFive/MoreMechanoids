using System.Linq;
using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class Building_PAL_Link : Building_PAL_Connectable
    {
        private static ThingDef PowerConduitDef { get { return ThingDef.Named("PowerConduit"); } }

        private int nextCheckTick = 1;

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // If deconstructed, make sure what is built on it is removed as well.
            if (mode != DestroyMode.Deconstruct) base.Destroy(mode);
            else
            {
                var things = Position.GetThingList();
                var palComponents = things.Where(t=>t is Building_PAL_Component).ToArray();
                foreach (var palComponent in palComponents)
                {
                    palComponent.Destroy(DestroyMode.Deconstruct);
                }

                var palBlueprints =
                    things
                        .Where(t => t is Blueprint_Build && t.def.entityDefToBuild.defName.StartsWith("PAL"))
                        .ToArray();
                foreach (var palBlueprint in palBlueprints)
                {
                    palBlueprint.Destroy(DestroyMode.Cancel);
                }
                base.Destroy(mode);
            }
        }

        // Not in use
        public void Expand()
        {
            nextCheckTick--;
            if (nextCheckTick > 0) return;
            nextCheckTick += 1;
            Log.Warning(Label+": "+Position);

            // Get connected cells
            var cell = Position.RandomAdjacentCellCardinal();

            if(cell.InBounds())
            {
                var conduit = cell.GetFirstThing(PowerConduitDef);
                if (conduit != null)
                {
                    if (Find.DesignationManager.DesignationOn(conduit, DesignationDefOf.Deconstruct) == null)
                    {
                        Find.DesignationManager.AddDesignation(new Designation(conduit, DesignationDefOf.Deconstruct));
                    }
                    var blueprint = cell.GetFirstThing(PowerConduitDef.blueprintDef);
                    if (blueprint == null)
                    {
                        GenConstruct.PlaceBlueprintForBuild(def, cell, new Rot4(), Faction.OfColony, GenStuff.DefaultStuffFor(def));
                    }
                }
            }
        }

    }
}