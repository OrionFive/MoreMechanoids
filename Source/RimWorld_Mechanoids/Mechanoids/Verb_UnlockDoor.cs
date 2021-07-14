using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MoreMechanoids
{
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
            if(!(currentTarget.Thing is Building_Door door)) return false;
            if (door.Open || door.IsForcedOpen())
            {
                CasterPawn.jobs.StopAll();
                return true;
            }
            if (!CanHitTarget(door))
            {
                Log.Warning(string.Concat(CasterPawn, " unlocked ", door, " from out of position."));
                return false;
            }
            CasterPawn.rotationTracker.Face(door.DrawPos);

            // Wood door ~100%, steel (160hp) ~69%, granite (270hp) ~41%, plasteel (450hp) ~24%
            var hpChance = 110.0f / Math.Max(door.HitPoints, 1);
            var unlockChance = Math.Min(GetNonMissChance(door), hpChance);
            
            if(Rand.Chance(unlockChance))
            {
                UnlockDoor(door);
            }
            else
            {
                SoundMiss().PlayOneShot(new TargetInfo(door.Position, door.MapHeld)); 
                CreateCombatLog(maneuver => maneuver.combatLogRulesMiss, false);
            }
            CasterPawn.Drawer.Notify_MeleeAttackOn(door);
            CasterPawn.rotationTracker.FaceCell(door.Position);
            CasterPawn.caller?.Notify_DidMeleeAttack();
            return true;
        }

        private void UnlockDoor(Building_Door door)
        {
            if (door.def.defName == "HeronInvisibleDoor")
            {
                ForceDoorExtended(door, base.Caster);
            }
            else
            {
                ForceDoor(door, base.Caster);
            }

            unlockDoorSound ??= SoundDef.Named("Explosion_EMP");

            unlockDoorSound.PlayOneShot(SoundInfo.InMap(CasterPawn));

            Vector3 loc = door.Position.ToVector3ShiftedWithAltitude(1);
            FleckMaker.ThrowMicroSparks(loc, door.Map);
            FleckMaker.ThrowLightningGlow(loc, door.Map, Rand.Range(0.7f, 1.5f));

            CasterPawn.jobs.StopAll();
        }

        private static void ForceDoor(Building_Door door, Thing instigator)
        {
            door.GetComp<CompForceable>().Force();
            door.TakeDamage(new DamageInfo(DamageDefOf.Crush, Rand.Gaussian(door.MaxHitPoints * 0.15f, 0.5f), 999, -1, instigator));
        }

        private static void ForceDoorExtended(Building_Door door, Thing instigator)
        {
            // Not a RimWorld field
            var building = Traverse.Create(door).Field("parentDoor").GetValue<Building>() as Building_Door;
            ForceDoor(building, instigator);
        }

        public override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            return new DamageWorker.DamageResult();
        }
    }
}
