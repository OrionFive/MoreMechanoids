using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class JobDriver_Standby : JobDriver
    {
        private const int TicksBetweenSleepZs = 100;
        private int ticksToSleepZ;
        private readonly string txtStandingBy = "BotStandingBy";

        //[DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Can't do condional - must always return the same amount
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell)
                    .FailOn(() => !this.pawn.CanReach(this.TargetA, PathEndMode.OnCell, Danger.Deadly))
                    .FailOn(() => {
                        Building_RestSpot spot = this.TargetA.Thing as Building_RestSpot;
                        if (spot == null) return false;
                        return spot.CurOccupant != null && spot.CurOccupant != this.pawn;
                    });

            yield return this.ToilStandby;

            //yield return Toils_Reserve.Release(TargetIndex.A).FailOn(()=>!Find.Reservations.);
        }

        public Toil ToilStandby
        {
            get
            {
                Toil toil = new Toil
                {
                    defaultCompleteMode = ToilCompleteMode.Delay,
                    defaultDuration = TicksBetweenSleepZs*Rand.Range(5, 15),
                    tickAction = this.ToilTick
                };

                //toil.initAction = () => { toil.actor.pather.StopDead(); };

                return toil;
            }
        }
        public bool StandingBy => this.HaveCurToil && this.CurToil.tickAction == this.ToilTick;
        private void ToilTick()
        {
            this.ticksToSleepZ --;
            if (this.ticksToSleepZ <= 0)
            {
                MoteMaker.ThrowMetaIcon(this.pawn.Position, this.pawn.Map, ThingDefOf.Mote_SleepZ);
                this.ticksToSleepZ += TicksBetweenSleepZs;
            }
        }

        public override string GetReport() => this.txtStandingBy;
    }
}
