using RimWorld;
using Verse;

namespace MoreMechanoids;

public class Verb_ParalyzingPoke : Verb_MeleeApplyHediff
{
    public override bool IsUsableOn(Thing target)
    {
        return GenericUtility.ValidSkullywagTargetPawn(target as Pawn);
    }

    public override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
    {
        var damageResult = new DamageWorker.DamageResult();
        if (target.Thing is not Pawn pawn)
        {
            return damageResult;
        }

        damageResult.AddHediff(pawn.health.AddHediff(HediffDef.Named("Paralyze")));
        damageResult.wounded = true;
        FleckMaker.ThrowMicroSparks(pawn.Drawer.DrawPos, pawn.Map);
        FleckMaker.ThrowLightningGlow(pawn.Drawer.DrawPos, pawn.Map, Rand.Range(0.7f, 1.5f));

        return damageResult;
    }
}