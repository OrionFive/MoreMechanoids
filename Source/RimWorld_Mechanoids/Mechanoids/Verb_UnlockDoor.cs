using System;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MoreMechanoids;

public class Verb_UnlockDoor : Verb_MeleeAttack
{
    private static SoundDef unlockDoorSound;

    public override bool TryCastShot()
    {
        if (!CasterPawn.Spawned)
        {
            return false;
        }

        if (CasterPawn.stances.FullBodyBusy)
        {
            return false;
        }

        //Log.Message("Trying open door on "+currentTarget.Thing);
        if (!AreAllowedThing(out var door))
        {
            return false;
        }

        if (AreDoorOpen(door))
        {
            CasterPawn.jobs.StopAll();
            return true;
        }

        if (!CanHitTarget(door))
        {
            Log.Warning($"{CasterPawn} unlocked {door} from out of position.");
            return false;
        }

        CasterPawn.rotationTracker.Face(door.DrawPos);

        // Wood door ~54%, steel (160hp) ~37%, granite (270hp) ~23%, plasteel (450hp) ~13%
        var hpChance = 60.0f / Math.Max(door.HitPoints, 1);
        var unlockChance = GetNonMissChance(door) * hpChance;

        if (Rand.Chance(unlockChance))
        {
            UnlockDoor(door);
        }
        else
        {
            SoundMiss().PlayOneShot(new TargetInfo(door.Position, door.MapHeld));
            CreateCombatLog(maneuverDef => maneuverDef.combatLogRulesMiss, false);
        }

        CasterPawn.Drawer.Notify_MeleeAttackOn(door);
        CasterPawn.rotationTracker.FaceCell(door.Position);
        CasterPawn.caller?.Notify_DidMeleeAttack();
        return true;
    }

    private void UnlockDoor(ThingWithComps door)
    {
        ForceDoor(door, base.Caster);

        CasterPawn.jobs.StopAll();
    }

    private static void ForceDoor([CanBeNull] ThingWithComps door, Thing instigator)
    {
        //Log.Message($"Forcing door: {door?.def.defName}, forceableComp = {door?.GetComp<CompForceable>() != null}");
        if (door == null)
        {
            return;
        }

        door.GetComp<CompForceable>()?.Force();
        door.TakeDamage(new DamageInfo(DamageDefOf.Crush, Rand.Gaussian(door.MaxHitPoints * 0.15f, 0.5f), 999, -1,
            instigator));

        unlockDoorSound ??= SoundDef.Named("Explosion_EMP");

        unlockDoorSound.PlayOneShot(SoundInfo.InMap(instigator));

        var loc = door.Position.ToVector3ShiftedWithAltitude(1);
        FleckMaker.ThrowMicroSparks(loc, door.Map);
        FleckMaker.ThrowLightningGlow(loc, door.Map, Rand.Range(0.7f, 1.5f));
    }

    protected virtual bool AreAllowedThing(out ThingWithComps door)
    {
        var isADoor = currentTarget.Thing is Building_Door;
        door = currentTarget.Thing as ThingWithComps;
        return isADoor;
    }

    protected virtual bool AreDoorOpen(ThingWithComps thing)
    {
        return thing is Building_Door door && (door.Open || door.IsForcedOpen());
    }

    public override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
    {
        return new DamageWorker.DamageResult();
    }

    public override bool IsUsableOn(Thing target)
    {
        return target is Building_Door;
    }
}