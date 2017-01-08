using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class IncidentWorker_ShortCircuitMechanoid : IncidentWorker
    {
        private readonly string txtShortCircuit = "BotShortCircuit".Translate();
        private readonly string txtShortCircuitLabel = "BotShortCircuitLabel".Translate();

        private static IEnumerable<PawnConverted> Mechanoids
        {
            get
            {
                return from Pawn pawn in Find.ListerPawns.PawnsInFaction(Faction.OfColony)
                    where !pawn.Dead && pawn.RaceProps.mechanoid && !pawn.health.Downed
                    select pawn as PawnConverted;
            }
        }

        protected override bool StorytellerCanUseNowSub()
        {
            return Mechanoids.Any(m => m.workCapacity < 0.3f);
        }

        public override bool TryExecute(IncidentParms parms)
        {
            //Log.Message("Triggered! Got mechanoids: "+Mechanoids.Count());
            var validTargets = Mechanoids.Where(m => m.workCapacity < 0.3f).ToArray();
            if (!validTargets.Any()) return false;
            //Log.Message("Getting lucky?");
            if (Rand.Value > validTargets.Count()*0.3f) return false;
            //Log.Message("Nope.");
            var target = validTargets.RandomElementByWeight(m => 1 - m.workCapacity);
            ShortCircuit(target);

            Find.History.AddHistoryRecordFromLetter(
                new Letter(txtShortCircuitLabel, string.Format(txtShortCircuit, target.NameStringShort),
                    LetterType.BadNonUrgent, target), string.Empty);
            return true;
        }

        private static void ShortCircuit(Thing hitThing)
        {
            if (hitThing == null) throw new NullReferenceException("hitThing");

            GenExplosion.DoExplosion(hitThing.Position, Rand.Range(1f, 3f), DamageDefOf.Flame, hitThing);
            if (hitThing != null)
            {
                hitThing.TryAttachFire(0.2f);
            }
            MoteThrower.ThrowStatic(hitThing.Position, ThingDefOf.Mote_ShotFlash, 6);
            MoteThrower.ThrowMicroSparks(hitThing.Position.ToVector3Shifted());

            if (!hitThing.Destroyed)
            {
                hitThing.TakeDamage(new DamageInfo(DamageDefOf.Stun, Rand.Range(150, 500), hitThing, null));
            }
        }
    }
}
