using Verse;

namespace MoreMechanoids
{
    public class DamageWorker_Paralyze : DamageWorker
    {
        public override DamageResult Apply(DamageInfo dInfo, Thing thing)
        {
            if (thing is Pawn pawn)
            {
                Hediff hediff = HediffMaker.MakeHediff(dInfo.Def.hediff, pawn);
                //hediff.Severity = dinfo.Amount;
                pawn.health.AddHediff(hediff, null, dInfo);
            }
            return new DamageResult();
        }
    }
}