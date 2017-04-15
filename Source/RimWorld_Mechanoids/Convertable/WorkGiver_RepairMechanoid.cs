using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class WorkGiver_RepairMechanoid : WorkGiver_Repair
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => pawn.Map.mapPawns.AllPawns.Where(IsRepairTarget).Select(p => (Thing)p);

        public override bool ShouldSkip(Pawn pawn) => !pawn.Map.mapPawns.AllPawns.Any(IsRepairTarget);

        public static bool IsRepairTarget(Thing t)
        {
            PawnConverted converted = t as PawnConverted;
            if (converted == null) return false;
            if (converted.Dead) return false;
            if (converted.Busy && !converted.Downed) return false;
            if (converted.IsBurning()) return false;
            // Damaged enough? Turn on full repair...
            if (converted.fullRepair) return true;
            if (!GetDamages(converted).Any()) return false;
            
            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            if (!IsRepairTarget(t))
            {
                return false;
            }
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            if (pawn.Faction == Faction.OfPlayer && !pawn.Map.areaManager.Home[t.Position])
            {
                if (t.Position.GetDangerFor(pawn) != Danger.None) return false;
            }
            PawnConverted converted = (PawnConverted) t;

            if (!pawn.CanReserve(converted)) return false;

            //var needed = converted.healthTracker.hediffSet.hediffs.Sum(h => Mathf.Max(1, h.Pain));
            //var available = Find.ResourceCounter.GetCount(ThingDefOf.Plasteel);
            //if (available*2 < needed) return false;
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t) => new Job(MoreMechanoidsDefOf.RepairMechanoid, t);

        public static IEnumerable<Hediff> GetDamages(PawnConverted converted) => converted.health.hediffSet.hediffs.Where(h => h.def.defName != "Offline" && IsDamagedEnough(h, converted.fullRepair));

        private static bool IsDamagedEnough(Hediff hediff, bool fullRepair)
        {
            if (fullRepair) return true;

            Hediff_Injury h = hediff as Hediff_Injury;
            if (h == null) return true;

            return h.Severity*3 > h.Part.def.hitPoints;
        }
    }
}