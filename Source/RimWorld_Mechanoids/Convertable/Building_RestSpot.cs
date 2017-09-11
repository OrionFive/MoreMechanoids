using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class Building_RestSpot : Building
    {
        public CompPowerTrader powerComp;
        //public Pawn owner;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        public Pawn CurOccupant
        {
            get
            {
                List<Thing> list = Map.thingGrid.ThingsListAt(Position);
                return list.OfType<Pawn>().FirstOrDefault(pawn => pawn.Position == Position);
            }
        }

        //public void Claim(PawnConverted pawn)
        //{
        //    var newBed = this;
        //    if (newBed.owner == pawn) return;
        //
        //    pawn.UnclaimSpot();
        //    if (newBed.owner != null)
        //    {
        //        newBed.owner.ownership.UnclaimBed();
        //    }
        //    newBed.owner = pawn;
        //    pawn.OwnedSpot = newBed;
        //    if (newBed.Medical)
        //    {
        //        Log.Warning(this.pawn.LabelCap + " claimed medical bed.");
        //        this.UnclaimBed();
        //    }
        //}

        public bool Fits(Pawn pawn)
        {
            // Huge bots need size 2.
            if (def.size.x >= 2 && def.size.z >= 2) return (pawn.def.race.baseBodySize >= 1.5f);
            return pawn.def.race.baseBodySize < 1.5f;
        }

        public static Building_RestSpot Find(Pawn pawn)
        {
            Building_RestSpot[] spots =
                pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_RestSpot>()
                    .Where(spot => (spot.CurOccupant == null || spot.CurOccupant == pawn) && spot.Fits(pawn) && !pawn.Map.reservationManager.IsReserved(spot, pawn.Faction))
                    .ToArray();

            if (!spots.Any()) return null;

            return spots.MinBy(spot => spot.Position.DistanceToSquared(pawn.Position)*(spot.powerComp.PowerOn ? 4 : 1));
        }
    }
}
