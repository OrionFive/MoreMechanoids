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

        private static IEnumerable<PawnConverted> Mechanoids(Map map)
        {
            return from Pawn pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer) where !pawn.Dead && pawn.RaceProps.IsMechanoid && !pawn.health.Downed select pawn as PawnConverted;
        }

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            return Mechanoids((Map) target).Any(m => m.workCapacity < 0.3f);
        }

        public override bool TryExecute(IncidentParms parms)
        {
            //Log.Message("Triggered! Got mechanoids: "+Mechanoids.Count());
            PawnConverted[] validTargets = Mechanoids((Map)parms.target).Where(m => m.workCapacity < 0.3f).ToArray();
            if (!validTargets.Any()) return false;
            //Log.Message("Getting lucky?");
            if (Rand.Value > validTargets.Length*0.3f) return false;
            //Log.Message("Nope.");
            PawnConverted target = validTargets.RandomElementByWeight(m => 1 - m.workCapacity);
            ShortCircuit(target);
            /**
             * TODO: Fix this shit
             */ 
            /*
            Find.History.AddHistoryRecordFromLetter(
                new Letter(txtShortCircuitLabel, string.Format(txtShortCircuit, target.NameStringShort),
                    LetterType.BadNonUrgent, target), string.Empty);
                    */
            return true;
        }

        private static void ShortCircuit(Thing hitThing)
        {
            if (hitThing == null) throw new NullReferenceException("hitThing");

            GenExplosion.DoExplosion(hitThing.Position, hitThing.Map, Rand.Range(1f, 3f), DamageDefOf.Flame, hitThing);
            if (hitThing != null)
            {
                hitThing.TryAttachFire(0.2f);
            }
            MoteMaker.MakeStaticMote(hitThing.Position, hitThing.Map, ThingDefOf.Mote_ShotFlash, 6);
            MoteMaker.ThrowMicroSparks(hitThing.Position.ToVector3Shifted(), hitThing.Map);

            if (!hitThing.Destroyed)
            {
                hitThing.TakeDamage(new DamageInfo(DamageDefOf.Stun, Rand.Range(150, 500)));
            }
        }
    }
}
