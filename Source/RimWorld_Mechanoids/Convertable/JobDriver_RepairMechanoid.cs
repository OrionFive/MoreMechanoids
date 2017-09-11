using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class JobDriver_RepairMechanoid : JobDriver
    {
        private const float repairAmount = 0.1f;
        private const int TicksBetweenRepairs = 7;
        protected float ticksToNextRepair;
        protected float repairingDone;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //yield return GetMaterial;
            //yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Goto;
            yield return ToilRepair;
            yield return Toils_Reserve.Release(TargetIndex.A);
        }

        public Toil GetMaterial
        {
            get
            {
                Toil toil = new Toil
                {
                    actor = pawn,
                    defaultCompleteMode = ToilCompleteMode.Instant,
                    initAction = () => {
                        if (!WorkGiver_RepairMechanoid.IsRepairTarget(TargetThingA))
                        {
                            EndJobWith(JobCondition.Incompletable);
                            return;
                        }
                        PawnConverted converted = (PawnConverted) TargetThingA;
                        float needed = WorkGiver_RepairMechanoid.GetDamages(converted).Sum(h => Mathf.Max(1, h.PainOffset)*80);
                        Log.Message("Fixing damages costs " + needed + " plasteel.");
                        int available = pawn.Map.resourceCounter.GetCount(ThingDefOf.Plasteel);
                        if (available*2 < needed)
                        {
                            EndJobWith(JobCondition.Incompletable);
                            return;
                        }
                        needed = Mathf.Min(needed, available);
                        // TODO: finish this
                    }
                };
                return toil;
            }
        }

        private Toil Goto
        {
            get
            {
                Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                toil.AddFailCondition(() => !WorkGiver_RepairMechanoid.IsRepairTarget(TargetThingA));
                return toil;
            }
        }

        public Toil ToilRepair
        {
            get
            {
                Toil toil = new Toil {actor = pawn, defaultCompleteMode = ToilCompleteMode.Never, tickAction = ToilTick};

                //toil.initAction = () => { toil.actor.pather.StopDead(); };
                toil.AddFailCondition(() => !WorkGiver_RepairMechanoid.IsRepairTarget(TargetThingA));
                return toil;
            }
        }

        private void ToilTick()
        {
            if (ticksToNextRepair-- > 0) return;
            ticksToNextRepair = TicksBetweenRepairs;

            TargetA.Thing.def.repairEffect.Spawn();

            PawnConverted converted = TargetThingA as PawnConverted;
            if (converted == null)
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            Hediff[] damages = WorkGiver_RepairMechanoid.GetDamages(converted).ToArray();
            if (damages.Length == 0)
            {
                EndJobWith(JobCondition.Succeeded);
                converted.SetFullRepair(false);
                converted.OnFullRepairComplete();
                return;
            }
            Hediff hediff = damages.RandomElement();
            repairingDone += repairAmount;
            if (repairingDone >= 3)
            {
                converted.health.hediffSet.hediffs.Remove(hediff);
                converted.health.Notify_HediffChanged(null);
                repairingDone = 0;
                converted.UpdateWorkCapacity();
            }
        }
    }
}
